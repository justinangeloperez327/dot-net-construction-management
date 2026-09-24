using Application.Common.Authentication;
using Application.Common.Files;
using Application.Projects;
using Domain.Documents;
using Domain.Projects;

namespace Application.Documents;

public sealed class CreateDocumentRevisionHandler(
    IDocumentRepository documents,
    IProjectRepository projects,
    IFileStorage fileStorage,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DocumentActionResult> HandleAsync(
        Guid documentId,
        CreateDocumentRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var document = await documents.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
        {
            return DocumentActionResult.Failure(
                "Document was not found.");
        }

        if (document.Status == DocumentStatus.Archived)
        {
            return DocumentActionResult.Failure(
                "Revisions cannot be added to an archived document.");
        }

        var project = await projects.GetByIdAsync(
            document.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return DocumentActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return DocumentActionResult.Failure(
                "Revisions cannot be added in a closed project.");
        }

        var validationError = FileUploadPolicy.ValidateAndGetExtension(
            request.FileName,
            request.ContentType,
            request.SizeBytes);

        if (validationError is not null)
        {
            return DocumentActionResult.Failure(validationError);
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DocumentActionResult.Failure(
                "An authenticated user is required.");
        }

        var revisionId = Guid.NewGuid();
        var safeFileName = Path.GetFileName(request.FileName.Trim());
        var extension = FileUploadPolicy.GetNormalizedExtension(
            safeFileName);
        var storageKey =
            $"documents/{document.Id:N}/revisions/{revisionId:N}{extension}";

        try
        {
            await fileStorage.WriteAsync(
                storageKey,
                request.Content,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is IOException or UnauthorizedAccessException)
        {
            return DocumentActionResult.Failure(
                "The revision file could not be stored.");
        }

        try
        {
            document.AddRevision(
                revisionId,
                request.RevisionCode,
                request.ChangeSummary,
                storageKey,
                safeFileName,
                request.ContentType.Trim().ToLowerInvariant(),
                request.SizeBytes,
                userId,
                timeProvider.GetUtcNow());

            await documents.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            await fileStorage.DeleteIfExistsAsync(
                storageKey,
                CancellationToken.None);

            return DocumentActionResult.Failure(exception.Message);
        }
        catch
        {
            await fileStorage.DeleteIfExistsAsync(
                storageKey,
                CancellationToken.None);

            throw;
        }

        return DocumentActionResult.Success(document.Id);
    }
}
