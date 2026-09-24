namespace Domain.Documents;

public sealed class DocumentRevision
{
    private DocumentRevision()
    {
    }

    internal DocumentRevision(
        Guid id,
        Guid documentId,
        string revisionCode,
        string? changeSummary,
        string storageKey,
        string fileName,
        string contentType,
        long sizeBytes,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Revision ID is required.",
                nameof(id));
        }

        if (documentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Document ID is required.",
                nameof(documentId));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Created-by user ID is required.",
                nameof(createdByUserId));
        }

        revisionCode = revisionCode.Trim();
        storageKey = storageKey.Trim();
        fileName = fileName.Trim();
        contentType = contentType.Trim();

        if (string.IsNullOrWhiteSpace(revisionCode))
        {
            throw new ArgumentException(
                "Revision code is required.",
                nameof(revisionCode));
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException(
                "Storage key is required.",
                nameof(storageKey));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException(
                "Content type is required.",
                nameof(contentType));
        }

        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sizeBytes),
                "File size must be greater than zero.");
        }

        Id = id;
        DocumentId = documentId;
        RevisionCode = revisionCode;
        ChangeSummary = NormalizeOptional(changeSummary);
        StorageKey = storageKey;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = DocumentRevisionStatus.Draft;
    }

    public Guid Id { get; private set; }

    public Guid DocumentId { get; private set; }

    public string RevisionCode { get; private set; } = string.Empty;

    public string? ChangeSummary { get; private set; }

    public DocumentRevisionStatus Status { get; private set; }

    public string StorageKey { get; private set; } = string.Empty;

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? SubmittedByUserId { get; private set; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAt { get; private set; }

    public string? ReviewComments { get; private set; }

    internal void Submit(
        Guid submittedByUserId,
        DateTimeOffset submittedAt)
    {
        if (Status != DocumentRevisionStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft revisions can be submitted.");
        }

        if (submittedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Submitted-by user ID is required.",
                nameof(submittedByUserId));
        }

        Status = DocumentRevisionStatus.Submitted;
        SubmittedByUserId = submittedByUserId;
        SubmittedAt = submittedAt;
    }

    internal void Review(
        DocumentRevisionReviewDecision decision,
        Guid reviewedByUserId,
        DateTimeOffset reviewedAt,
        string? comments)
    {
        if (Status != DocumentRevisionStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted revisions can be reviewed.");
        }

        if (reviewedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Reviewed-by user ID is required.",
                nameof(reviewedByUserId));
        }

        comments = NormalizeOptional(comments);

        if ((decision is DocumentRevisionReviewDecision.ApproveWithComments
                or DocumentRevisionReviewDecision.Reject) &&
            comments is null)
        {
            throw new ArgumentException(
                "Review comments are required for this decision.",
                nameof(comments));
        }

        Status = decision switch
        {
            DocumentRevisionReviewDecision.Approve =>
                DocumentRevisionStatus.Approved,
            DocumentRevisionReviewDecision.ApproveWithComments =>
                DocumentRevisionStatus.ApprovedWithComments,
            DocumentRevisionReviewDecision.Reject =>
                DocumentRevisionStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(decision))
        };

        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = reviewedAt;
        ReviewComments = comments;
    }

    internal void MarkSuperseded()
    {
        if (Status is DocumentRevisionStatus.Approved
            or DocumentRevisionStatus.ApprovedWithComments)
        {
            Status = DocumentRevisionStatus.Superseded;
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
