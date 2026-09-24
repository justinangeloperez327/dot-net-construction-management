using Application.Users;

namespace Application.Documents;

public sealed class GetDocumentHandler(
    IDocumentRepository documents,
    IUserDirectory users)
{
    public async Task<DocumentDetails?> HandleAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await documents.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        var directory = await users.ListByIdsAsync(
            [document.CreatedByUserId],
            cancellationToken);

        var createdBy = directory
            .SingleOrDefault(
                user => user.Id == document.CreatedByUserId)
            ?.Email
            ?? "Unknown user";

        return new DocumentDetails(
            document.Id,
            document.ProjectId,
            document.DocumentNumber,
            document.Title,
            document.Category,
            document.Discipline,
            document.Originator,
            document.Description,
            document.Status,
            createdBy,
            document.CreatedAt);
    }
}
