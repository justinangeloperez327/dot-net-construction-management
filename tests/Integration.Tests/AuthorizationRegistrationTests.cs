using Application.Common.Authorization;
using Application.Roles;
using Application.Users;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class AuthorizationRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Every_permission_has_an_authorization_policy()
    {
        using var scope = _factory.Services.CreateScope();

        var provider = scope.ServiceProvider
            .GetRequiredService<IAuthorizationPolicyProvider>();

        foreach (var permission in Permissions.All)
        {
            var policy = await provider.GetPolicyAsync(permission);

            Assert.NotNull(policy);
        }
    }

    [Fact]
    public void Administration_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IUserAdministration>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IRoleAdministration>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ApplicationSignInManager>());
    }

    [Fact]
    public void Application_user_is_active_by_default()
    {
        var user = new ApplicationUser();

        Assert.True(user.IsActive);
    }

    [Fact]
    public void Permissions_are_unique()
    {
        Assert.Equal(
            Permissions.All.Count,
            Permissions.All.Distinct(StringComparer.Ordinal).Count());
    }
}
