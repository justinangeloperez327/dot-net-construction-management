using Application.Projects;
using Domain.Projects;

namespace Application.Documents;

public sealed record UpdateDocumentRequest(
    string DocumentNumber,
    string Title,
    string? Category,
    string? Discipline,
    string? Originator,
    string? Description);

public sealed class UpdateDocumentHandler(
    IDocumentRepository documents,
    IProjectRepository projects)
{
    public async Task<DocumentActionResult> HandleAsync(
        Guid documentId,
        UpdateDocumentRequest request,
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

        if (project is null)
        {
            return DocumentActionResult.Failure(
                "Project was not found.");
        }

        if (project.Status == ProjectStatus.Closed)
        {
            return DocumentActionResult.Failure(
                "Documents in a closed project cannot be changed.");
        }

        var documentNumber = request.DocumentNumber.Trim();

        if (await documents.DocumentNumberExistsAsync(
                document.ProjectId,
                documentNumber,
                document.Id,
                cancellationToken))
        {
            return DocumentActionResult.Failure(
                "A document with this number already exists in the project.");
        }

        try
        {
            document.Update(
                documentNumber,
                request.Title,
                request.Category,
                request.Discipline,
                request.Originator,
                request.Description);

            await documents.SaveChangesAsync(cancellationToken);

            return DocumentActionResult.Success(document.Id);
        }
        catch (Exception exception)
            when (exception is ArgumentException or InvalidOperationException)
        {
            return DocumentActionResult.Failure(exception.Message);
        }
    }
}
