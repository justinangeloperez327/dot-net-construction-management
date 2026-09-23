namespace Application.Projects;

public sealed record ProjectMemberDetails(
    Guid UserId,
    string Email,
    string? Responsibility);

public sealed record ProjectMemberActionResult(
    bool Succeeded,
    IReadOnlyList<string> Errors)
{
    public static ProjectMemberActionResult Success() =>
        new(true, []);

    public static ProjectMemberActionResult Failure(
        params string[] errors) =>
        new(false, errors);
}
