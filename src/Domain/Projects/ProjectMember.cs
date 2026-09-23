namespace Domain.Projects;

public sealed class ProjectMember
{
    private ProjectMember()
    {
    }

    private ProjectMember(
        Guid projectId,
        Guid userId,
        string? responsibility)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        ProjectId = projectId;
        UserId = userId;
        Responsibility = NormalizeOptional(responsibility);
    }

    public Guid ProjectId { get; private set; }

    public Guid UserId { get; private set; }

    public string? Responsibility { get; private set; }

    public static ProjectMember Assign(
        Guid projectId,
        Guid userId,
        string? responsibility)
    {
        return new ProjectMember(
            projectId,
            userId,
            responsibility);
    }

    public void UpdateResponsibility(string? responsibility)
    {
        Responsibility = NormalizeOptional(responsibility);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
