using Application.Common.Authentication;
using Application.Projects;
using Domain.Documents;
using Domain.Projects;

namespace Application.Documents;

public sealed class SubmitDocumentRevisionHandler(
    IDocumentRepository documents,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DocumentActionResult> HandleAsync(
        Guid documentId,
        Guid revisionId,
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

        var project = await projects.GetByIdAsync(
            document.ProjectId,
            cancellationToken);

        if (project is null ||
            project.Status == ProjectStatus.Closed)
        {
            return DocumentActionResult.Failure(
                "Revisions in a closed or missing project cannot be submitted.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DocumentActionResult.Failure(
                "An authenticated user is required.");
        }

        try
        {
            document.SubmitRevision(
                revisionId,
                userId,
                timeProvider.GetUtcNow());

            await documents.SaveChangesAsync(cancellationToken);

            return DocumentActionResult.Success(document.Id);
        }
        catch (InvalidOperationException exception)
        {
            return DocumentActionResult.Failure(exception.Message);
        }
    }
}

public sealed class ReviewDocumentRevisionHandler(
    IDocumentRepository documents,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DocumentActionResult> HandleAsync(
        Guid documentId,
        Guid revisionId,
        ReviewDocumentRevisionRequest request,
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

        var project = await projects.GetByIdAsync(
            document.ProjectId,
            cancellationToken);

        if (project is null ||
            project.Status == ProjectStatus.Closed)
        {
            return DocumentActionResult.Failure(
                "Revisions in a closed or missing project cannot be reviewed.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DocumentActionResult.Failure(
                "An authenticated reviewer is required.");
        }

        try
        {
            document.ReviewRevision(
                revisionId,
                request.Decision,
                userId,
                timeProvider.GetUtcNow(),
                request.Comments);

            await documents.SaveChangesAsync(cancellationToken);

            return DocumentActionResult.Success(document.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException
                or InvalidOperationException)
        {
            return DocumentActionResult.Failure(exception.Message);
        }
    }
}
