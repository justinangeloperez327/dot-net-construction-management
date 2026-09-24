using Application.Common.Authentication;
using Application.Projects;
using Domain.Documents;
using Domain.Projects;

namespace Application.Documents;

public sealed record CreateDocumentRequest(
    Guid ProjectId,
    string DocumentNumber,
    string Title,
    string? Category,
    string? Discipline,
    string? Originator,
    string? Description);

public sealed class CreateDocumentHandler(
    IDocumentRepository documents,
    IProjectRepository projects,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<DocumentActionResult> HandleAsync(
        CreateDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return DocumentActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return DocumentActionResult.Failure(
                "Documents cannot be created for a closed project.");
        }

        var documentNumber = request.DocumentNumber.Trim();

        if (await documents.DocumentNumberExistsAsync(
                request.ProjectId,
                documentNumber,
                cancellationToken: cancellationToken))
        {
            return DocumentActionResult.Failure(
                "A document with this number already exists in the project.");
        }

        var user = await currentUser.GetAsync(cancellationToken);

        if (!user.IsAuthenticated || user.UserId is not Guid userId)
        {
            return DocumentActionResult.Failure(
                "An authenticated user is required.");
        }

        try
        {
            var document = Document.Create(
                request.ProjectId,
                documentNumber,
                request.Title,
                request.Category,
                request.Discipline,
                request.Originator,
                request.Description,
                userId,
                timeProvider.GetUtcNow());

            await documents.AddAsync(document, cancellationToken);
            await documents.SaveChangesAsync(cancellationToken);

            return DocumentActionResult.Success(document.Id);
        }
        catch (ArgumentException exception)
        {
            return DocumentActionResult.Failure(exception.Message);
        }
    }
}
