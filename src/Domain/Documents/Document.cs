namespace Domain.Documents;

public sealed class Document
{
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

    public void Archive()
    {
        Status = DocumentStatus.Archived;
    }

    public void Restore()
    {
        Status = DocumentStatus.Active;
    }

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
                "Archived documents cannot be edited.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
