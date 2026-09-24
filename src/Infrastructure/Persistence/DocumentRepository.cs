using Application.Documents;
using Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class DocumentRepository(
    ApplicationDbContext dbContext)
    : IDocumentRepository
{
    public Task<Document?> GetByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Documents
            .SingleOrDefaultAsync(
                document => document.Id == documentId,
                cancellationToken);
    }

    public Task<bool> DocumentNumberExistsAsync(
        Guid projectId,
        string documentNumber,
        Guid? excludingDocumentId = null,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Documents.AnyAsync(
            document =>
                document.ProjectId == projectId &&
                document.DocumentNumber == documentNumber &&
                (!excludingDocumentId.HasValue ||
                 document.Id != excludingDocumentId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> ListForProjectAsync(
        Guid projectId,
        string? search,
        DocumentStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(
                dbContext.Documents
                    .AsNoTracking()
                    .Where(document => document.ProjectId == projectId),
                search,
                status)
            .OrderBy(document => document.DocumentNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForProjectAsync(
        Guid projectId,
        string? search,
        DocumentStatus? status,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilter(
                dbContext.Documents
                    .AsNoTracking()
                    .Where(document => document.ProjectId == projectId),
                search,
                status)
            .CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        Document document,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Documents.AddAsync(
            document,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Document> ApplyFilter(
        IQueryable<Document> query,
        string? search,
        DocumentStatus? status)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(document =>
                document.DocumentNumber.Contains(search) ||
                document.Title.Contains(search) ||
                (document.Category != null &&
                 document.Category.Contains(search)) ||
                (document.Discipline != null &&
                 document.Discipline.Contains(search)) ||
                (document.Originator != null &&
                 document.Originator.Contains(search)));
        }

        if (status is not null)
        {
            query = query.Where(
                document => document.Status == status);
        }

        return query;
    }
}
