namespace Application.Roles;

public interface IRoleAdministration
{
    Task<IReadOnlyList<RoleSummary>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<RoleDetails?> GetAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<RoleActionResult> CreateAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<RoleActionResult> RenameAsync(
        Guid roleId,
        string name,
        CancellationToken cancellationToken = default);

    Task<RoleActionResult> DeleteAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    Task<RoleActionResult> SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken = default);
}

public sealed record RoleSummary(
    Guid Id,
    string Name);

public sealed record RoleDetails(
    Guid Id,
    string Name,
    IReadOnlyList<string> Permissions,
    bool IsSystemRole);

public sealed record RoleActionResult(
    bool Succeeded,
    Guid? RoleId,
    IReadOnlyList<string> Errors)
{
    public static RoleActionResult Success(Guid roleId) =>
        new(true, roleId, []);

    public static RoleActionResult Failure(params string[] errors) =>
        new(false, null, errors);

    public static RoleActionResult Failure(IEnumerable<string> errors) =>
        new(false, null, errors.ToArray());
}
