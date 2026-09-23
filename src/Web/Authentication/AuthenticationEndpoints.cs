using Infrastructure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;

namespace Web.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/account/login",
            LoginAsync);

        endpoints.MapPost(
            "/account/logout",
            LogoutAsync);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var form = await httpContext.Request.ReadFormAsync(
            httpContext.RequestAborted);

        var email = GetValue(form["email"]);
        var password = GetValue(form["password"]);
        var returnUrl = GetSafeReturnUrl(GetValue(form["returnUrl"]));
        var rememberMe = string.Equals(
            GetValue(form["rememberMe"]),
            "on",
            StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return RedirectToLogin(returnUrl, "invalid");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return RedirectToLogin(returnUrl, "invalid");
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        var error = result.IsLockedOut
            ? "locked"
            : "invalid";

        return RedirectToLogin(returnUrl, error);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        await antiforgery.ValidateRequestAsync(httpContext);
        await signInManager.SignOutAsync();

        return Results.LocalRedirect("/login");
    }

    private static IResult RedirectToLogin(
        string returnUrl,
        string error)
    {
        var query = QueryString.Create(
            [
                new KeyValuePair<string, string?>("returnUrl", returnUrl),
                new KeyValuePair<string, string?>("error", error)
            ]);

        return Results.LocalRedirect($"/login{query}");
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !returnUrl.StartsWith('/', StringComparison.Ordinal) ||
            returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }

    private static string? GetValue(StringValues values)
    {
        return values.Count > 0
            ? values[0]
            : null;
    }
}
