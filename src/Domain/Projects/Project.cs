namespace Domain.Projects;

public sealed class Project
{
    private Project()
    {
    }

    private Project(
        Guid id,
        string projectNumber,
        string name,
        string? location,
        DateOnly startDate,
        DateOnly? targetCompletionDate,
        string? description)
    {
        Id = id;
        Status = ProjectStatus.Active;

        SetDetails(
            projectNumber,
            name,
            location,
            startDate,
            targetCompletionDate,
            description);
    }

    public Guid Id { get; private set; }

    public string ProjectNumber { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public Guid? ClientId { get; private set; }

    public string? Location { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? TargetCompletionDate { get; private set; }

    public string? Description { get; private set; }

    public ProjectStatus Status { get; private set; }

    public static Project Create(
        string projectNumber,
        string name,
        string? location,
        DateOnly startDate,
        DateOnly? targetCompletionDate,
        string? description)
    {
        return new Project(
            Guid.NewGuid(),
            projectNumber,
            name,
            location,
            startDate,
            targetCompletionDate,
            description);
    }

    public void Update(
        string projectNumber,
        string name,
        string? location,
        DateOnly startDate,
        DateOnly? targetCompletionDate,
        string? description)
    {
        EnsureActive();

        SetDetails(
            projectNumber,
            name,
            location,
            startDate,
            targetCompletionDate,
            description);
    }

    public void AssignClient(Guid? clientId)
    {
        EnsureActive();

        if (clientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Client ID cannot be empty.",
                nameof(clientId));
        }

        ClientId = clientId;
    }

    public void Close()
    {
        Status = ProjectStatus.Closed;
    }

    private void EnsureActive()
    {
        if (Status == ProjectStatus.Closed)
        {
            throw new InvalidOperationException(
                "Closed projects cannot be changed.");
        }
    }

    private void SetDetails(
        string projectNumber,
        string name,
        string? location,
        DateOnly startDate,
        DateOnly? targetCompletionDate,
        string? description)
    {
        projectNumber = projectNumber.Trim();
        name = name.Trim();
        location = NormalizeOptional(location);
        description = NormalizeOptional(description);

        if (string.IsNullOrWhiteSpace(projectNumber))
        {
            throw new ArgumentException(
                "Project number is required.",
                nameof(projectNumber));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Project name is required.",
                nameof(name));
        }

        if (targetCompletionDate is not null &&
            targetCompletionDate < startDate)
        {
            throw new ArgumentException(
                "Target completion date cannot be before the start date.",
                nameof(targetCompletionDate));
        }

        ProjectNumber = projectNumber;
        Name = name;
        Location = location;
        StartDate = startDate;
        TargetCompletionDate = targetCompletionDate;
        Description = description;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
