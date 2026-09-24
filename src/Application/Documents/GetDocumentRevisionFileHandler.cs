using Application.Common.Files;

namespace Application.Documents;

public sealed class GetDocumentRevisionFileHandler(
    IDocumentRepository documents,
    IFileStorage fileStorage)
{
    public async Task<DocumentRevisionFile?> HandleAsync(
        Guid documentId,
        Guid revisionId,
        CancellationToken cancellationToken = default)
    {
        var document = await documents.GetByIdAsync(
            documentId,
            cancellationToken);

        var revision = document?.Revisions.SingleOrDefault(
            item => item.Id == revisionId);

        if (revision is null)
        {
            return null;
        }

        var content = await fileStorage.OpenReadAsync(
            revision.StorageKey,
            cancellationToken);

        return content is null
            ? null
            : new DocumentRevisionFile(
                content,
                revision.FileName,
                revision.ContentType);
    }
}
