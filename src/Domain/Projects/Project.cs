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
        if (Status == ProjectStatus.Closed)
        {
            throw new InvalidOperationException(
                "Closed projects cannot be updated.");
        }

        SetDetails(
            projectNumber,
            name,
            location,
            startDate,
            targetCompletionDate,
            description);
    }

    public void Close()
    {
        Status = ProjectStatus.Closed;
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
