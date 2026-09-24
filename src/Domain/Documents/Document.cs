namespace Domain.Documents;

public sealed class Document
{
    private readonly List<DocumentRevision> _revisions = [];

    private Document()
    {
    }

    private Document(
        Guid projectId,
        string documentNumber,
        string title,
        string? category,
        string? discipline,
        string? originator,
        string? description,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Created-by user ID is required.",
                nameof(createdByUserId));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = DocumentStatus.Active;

        SetDetails(
            documentNumber,
            title,
            category,
            discipline,
            originator,
            description);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string DocumentNumber { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public string? Discipline { get; private set; }

    public string? Originator { get; private set; }

    public string? Description { get; private set; }

    public DocumentStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<DocumentRevision> Revisions =>
        _revisions;

    public static Document Create(
        Guid projectId,
        string documentNumber,
        string title,
        string? category,
        string? discipline,
        string? originator,
        string? description,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        return new Document(
            projectId,
            documentNumber,
            title,
            category,
            discipline,
            originator,
            description,
            createdByUserId,
            createdAt);
    }

    public void Update(
        string documentNumber,
        string title,
        string? category,
        string? discipline,
        string? originator,
        string? description)
    {
        EnsureActive();

        SetDetails(
            documentNumber,
            title,
            category,
            discipline,
            originator,
            description);
    }

    public DocumentRevision AddRevision(
        Guid revisionId,
        string revisionCode,
        string? changeSummary,
        string storageKey,
        string fileName,
        string contentType,
        long sizeBytes,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        EnsureActive();

        revisionCode = revisionCode.Trim();

        if (_revisions.Any(revision =>
                string.Equals(
                    revision.RevisionCode,
                    revisionCode,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "This revision code already exists for the document.");
        }

        if (_revisions.Any(revision =>
                revision.Status is DocumentRevisionStatus.Draft
                    or DocumentRevisionStatus.Submitted))
        {
            throw new InvalidOperationException(
                "Complete the current draft or submitted revision before creating another revision.");
        }

        var revision = new DocumentRevision(
            revisionId,
            Id,
            revisionCode,
            changeSummary,
            storageKey,
            fileName,
            contentType,
            sizeBytes,
            createdByUserId,
            createdAt);

        _revisions.Add(revision);

        return revision;
    }

    public void SubmitRevision(
        Guid revisionId,
        Guid submittedByUserId,
        DateTimeOffset submittedAt)
    {
        EnsureActive();

        FindRevision(revisionId).Submit(
            submittedByUserId,
            submittedAt);
    }

    public void ReviewRevision(
        Guid revisionId,
        DocumentRevisionReviewDecision decision,
        Guid reviewedByUserId,
        DateTimeOffset reviewedAt,
        string? comments)
    {
        EnsureActive();

        var revision = FindRevision(revisionId);

        revision.Review(
            decision,
            reviewedByUserId,
            reviewedAt,
            comments);

        if (revision.Status is DocumentRevisionStatus.Approved
            or DocumentRevisionStatus.ApprovedWithComments)
        {
            foreach (var previous in _revisions.Where(
                         item => item.Id != revision.Id))
            {
                previous.MarkSuperseded();
            }
        }
    }

    public void Archive()
    {
        if (_revisions.Any(revision =>
                revision.Status is DocumentRevisionStatus.Draft
                    or DocumentRevisionStatus.Submitted))
        {
            throw new InvalidOperationException(
                "A document with a draft or submitted revision cannot be archived.");
        }

        Status = DocumentStatus.Archived;
    }

    public void Restore()
    {
        Status = DocumentStatus.Active;
    }

    private DocumentRevision FindRevision(Guid revisionId) =>
        _revisions.SingleOrDefault(
            revision => revision.Id == revisionId)
        ?? throw new InvalidOperationException(
            "Document revision was not found.");

    private void SetDetails(
        string documentNumber,
        string title,
        string? category,
        string? discipline,
        string? originator,
        string? description)
    {
        documentNumber = documentNumber.Trim();
        title = title.Trim();

        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            throw new ArgumentException(
                "Document number is required.",
                nameof(documentNumber));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Document title is required.",
                nameof(title));
        }

        DocumentNumber = documentNumber;
        Title = title;
        Category = NormalizeOptional(category);
        Discipline = NormalizeOptional(discipline);
        Originator = NormalizeOptional(originator);
        Description = NormalizeOptional(description);
    }

    private void EnsureActive()
    {
        if (Status == DocumentStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived documents cannot be changed.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
