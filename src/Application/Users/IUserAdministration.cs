namespace Application.Users;

public interface IUserAdministration
{
    Task<IReadOnlyList<UserSummary>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<UserDetails?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserActionResult> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserActionResult> UpdateAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserActionResult> SetActiveAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<UserActionResult> SetRolesAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}

public sealed record UserSummary(
    Guid Id,
    string Email,
    bool IsActive);

public sealed record UserDetails(
    Guid Id,
    string Email,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record CreateUserRequest(
    string Email,
    string Password,
    IReadOnlyCollection<string> Roles);

public sealed record UpdateUserRequest(
    string Email);

public sealed record UserActionResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyList<string> Errors)
{
    public static UserActionResult Success(Guid userId) =>
        new(true, userId, []);

    public static UserActionResult Failure(params string[] errors) =>
        new(false, null, errors);

    public static UserActionResult Failure(IEnumerable<string> errors) =>
        new(false, null, errors.ToArray());
}
