using Domain.Documents;

namespace Application.Documents;

public sealed record DocumentSummary(
    Guid Id,
    string DocumentNumber,
    string Title,
    string? Category,
    string? Discipline,
    string? Originator,
    DocumentStatus Status);

public sealed record DocumentDetails(
    Guid Id,
    Guid ProjectId,
    string DocumentNumber,
    string Title,
    string? Category,
    string? Discipline,
    string? Originator,
    string? Description,
    DocumentStatus Status,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    IReadOnlyList<DocumentRevisionDetails> Revisions);

public sealed record DocumentListResult(
    IReadOnlyList<DocumentSummary> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages =>
        Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed record DocumentActionResult(
    bool Succeeded,
    Guid? DocumentId,
    IReadOnlyList<string> Errors)
{
    public static DocumentActionResult Success(Guid documentId) =>
        new(true, documentId, []);

    public static DocumentActionResult Failure(params string[] errors) =>
        new(false, null, errors);
}
