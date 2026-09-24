using Application.Projects;
using Domain.Projects;

namespace Application.Documents;

public sealed class SetDocumentArchivedHandler(
    IDocumentRepository documents,
    IProjectRepository projects)
{
    public async Task<DocumentActionResult> HandleAsync(
        Guid documentId,
        bool archived,
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

        if (archived)
        {
            document.Archive();
        }
        else
        {
            document.Restore();
        }

        await documents.SaveChangesAsync(cancellationToken);

        return DocumentActionResult.Success(document.Id);
    }
}
