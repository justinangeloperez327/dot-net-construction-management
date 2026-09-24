using Domain.Documents;

namespace Application.Documents;

public sealed record DocumentRevisionDetails(
    Guid Id,
    string RevisionCode,
    string? ChangeSummary,
    DocumentRevisionStatus Status,
    string FileName,
    string ContentType,
    long SizeBytes,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    string? SubmittedBy,
    DateTimeOffset? SubmittedAt,
    string? ReviewedBy,
    DateTimeOffset? ReviewedAt,
    string? ReviewComments);

public sealed record CreateDocumentRevisionRequest(
    string RevisionCode,
    string? ChangeSummary,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

public sealed record ReviewDocumentRevisionRequest(
    DocumentRevisionReviewDecision Decision,
    string? Comments);

public sealed record DocumentRevisionFile(
    Stream Content,
    string FileName,
    string ContentType);
