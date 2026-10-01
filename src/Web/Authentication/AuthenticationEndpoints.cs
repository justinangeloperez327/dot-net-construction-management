using System.Security.Claims;
using Application.Security;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using Web.Security;

namespace Web.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/account/login",
                LoginAsync)
            .RequireRateLimiting(
                SecurityPolicyNames.Authentication);

        endpoints.MapPost(
                "/account/logout",
                LogoutAsync)
            .RequireRateLimiting(
                SecurityPolicyNames.Authentication);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        UserManager<ApplicationUser> userManager,
        ApplicationSignInManager signInManager,
        ISecurityEventRecorder securityEvents,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var form = await httpContext.Request.ReadFormAsync(
            httpContext.RequestAborted);

        var email = GetValue(form["email"]);
        var password = GetValue(form["password"]);
        var returnUrl = GetSafeReturnUrl(
            GetValue(form["returnUrl"]));
        var rememberMe = string.Equals(
            GetValue(form["rememberMe"]),
            "on",
            StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            await TryRecordAsync(
                securityEvents,
                BuildEvent(
                    httpContext,
                    timeProvider,
                    SecurityEventType.LoginFailed,
                    null,
                    email),
                loggerFactory,
                httpContext.RequestAborted);

            return RedirectToLogin(returnUrl, "invalid");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            await TryRecordAsync(
                securityEvents,
                BuildEvent(
                    httpContext,
                    timeProvider,
                    SecurityEventType.LoginFailed,
                    user?.Id,
                    user?.Email ?? email),
                loggerFactory,
                httpContext.RequestAborted);

            return RedirectToLogin(returnUrl, "invalid");
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);

        var eventType = result.Succeeded
            ? SecurityEventType.LoginSucceeded
            : result.IsLockedOut
                ? SecurityEventType.LoginLockedOut
                : SecurityEventType.LoginFailed;

        await TryRecordAsync(
            securityEvents,
            BuildEvent(
                httpContext,
                timeProvider,
                eventType,
                user.Id,
                user.Email),
            loggerFactory,
            httpContext.RequestAborted);

        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        return RedirectToLogin(
            returnUrl,
            result.IsLockedOut ? "locked" : "invalid");
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ApplicationSignInManager signInManager,
        ISecurityEventRecorder securityEvents,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        Guid? userId = Guid.TryParse(
            httpContext.User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            out var parsedUserId)
            ? parsedUserId
            : null;

        var email =
            httpContext.User.FindFirstValue(ClaimTypes.Email)
            ?? httpContext.User.Identity?.Name;

        await signInManager.SignOutAsync();

        await TryRecordAsync(
            securityEvents,
            BuildEvent(
                httpContext,
                timeProvider,
                SecurityEventType.Logout,
                userId,
                email),
            loggerFactory,
            httpContext.RequestAborted);

        return Results.LocalRedirect("/login");
    }

    private static SecurityEventRecord BuildEvent(
        HttpContext context,
        TimeProvider timeProvider,
        SecurityEventType eventType,
        Guid? userId,
        string? email) =>
        new(
            eventType,
            userId,
            email,
            context.Connection.RemoteIpAddress?.ToString(),
            context.Request.Headers.UserAgent.ToString(),
            context.Request.Path,
            timeProvider.GetUtcNow());

    private static async Task TryRecordAsync(
        ISecurityEventRecorder securityEvents,
        SecurityEventRecord securityEvent,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            await securityEvents.RecordAsync(
                securityEvent,
                cancellationToken);
        }
        catch (Exception exception)
        {
            loggerFactory
                .CreateLogger("SecurityEvents")
                .LogWarning(
                    exception,
                    "Security event {EventType} could not be persisted.",
                    securityEvent.EventType);
        }
    }

    private static IResult RedirectToLogin(
        string returnUrl,
        string error)
    {
        var query = QueryString.Create(
            [
                new KeyValuePair<string, string?>(
                    "returnUrl",
                    returnUrl),
                new KeyValuePair<string, string?>(
                    "error",
                    error)
            ]);

        return Results.LocalRedirect($"/login{query}");
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !returnUrl.StartsWith("/", StringComparison.Ordinal) ||
            returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }

    private static string? GetValue(StringValues values) =>
        values.Count > 0
            ? values[0]
            : null;
}
