using System.Security.Claims;
using Application.Common.Authentication;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Authentication;

public sealed class CurrentUser(
    AuthenticationStateProvider authenticationStateProvider)
    : ICurrentUser
{
    public async ValueTask<CurrentUserInfo> GetAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var authenticationState =
            await authenticationStateProvider.GetAuthenticationStateAsync();

        var principal = authenticationState.User;
        var authenticated = principal.Identity?.IsAuthenticated == true;

        Guid? userId = null;

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var email =
            principal.FindFirstValue(ClaimTypes.Email) ??
            principal.Identity?.Name;

        return new CurrentUserInfo(
            userId,
            email,
            authenticated);
    }
}
