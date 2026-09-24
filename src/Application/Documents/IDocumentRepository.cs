using Domain.Documents;

namespace Application.Documents;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task<bool> DocumentNumberExistsAsync(
        Guid projectId,
        string documentNumber,
        Guid? excludingDocumentId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> ListForProjectAsync(
        Guid projectId,
        string? search,
        DocumentStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountForProjectAsync(
        Guid projectId,
        string? search,
        DocumentStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Document document,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
