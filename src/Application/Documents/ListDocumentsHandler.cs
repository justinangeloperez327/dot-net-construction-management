using Domain.Documents;

namespace Application.Documents;

public sealed record ListDocumentsRequest(
    Guid ProjectId,
    string? Search = null,
    DocumentStatus? Status = null,
    int Page = 1,
    int PageSize = 25);

public sealed class ListDocumentsHandler(
    IDocumentRepository documents)
{
    public async Task<DocumentListResult> HandleAsync(
        ListDocumentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        var totalCount = await documents.CountForProjectAsync(
            request.ProjectId,
            search,
            request.Status,
            cancellationToken);

        var items = await documents.ListForProjectAsync(
            request.ProjectId,
            search,
            request.Status,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);

        return new DocumentListResult(
            items.Select(document => new DocumentSummary(
                    document.Id,
                    document.DocumentNumber,
                    document.Title,
                    document.Category,
                    document.Discipline,
                    document.Originator,
                    document.Status))
                .ToArray(),
            page,
            pageSize,
            totalCount);
    }
}
