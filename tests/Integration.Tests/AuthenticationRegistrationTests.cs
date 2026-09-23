using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class AuthenticationRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Identity_uses_application_cookie_as_default_scheme()
    {
        using var scope = _factory.Services.CreateScope();

        var schemeProvider = scope.ServiceProvider
            .GetRequiredService<IAuthenticationSchemeProvider>();

        var scheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();

        Assert.NotNull(scheme);
        Assert.Equal(
            IdentityConstants.ApplicationScheme,
            scheme.Name);
    }

    [Fact]
    public void Identity_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<UserManager<ApplicationUser>>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<SignInManager<ApplicationUser>>());
    }
}
