using Application.Common.Authorization;
using Application.Security;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Web.Authentication;
using Xunit;

namespace Integration.Tests;

public sealed class SecurityHardeningTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SecurityHardeningTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Security_services_and_permission_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<ISecurityEventRecorder>());
        Assert.NotNull(
            scope.ServiceProvider.GetService<ISecurityEventQueryService>());
        Assert.NotNull(
            scope.ServiceProvider.GetService<ListSecurityEventsHandler>());

        var provider = scope.ServiceProvider
            .GetRequiredService<AuthenticationStateProvider>();

        Assert.IsType<
            IdentityRevalidatingAuthenticationStateProvider>(
            provider);

        Assert.Contains(
            Permissions.Security.ViewEvents,
            Permissions.All);
    }

    [Fact]
    public void Identity_and_cookie_options_are_hardened()
    {
        using var scope = _factory.Services.CreateScope();

        var identity = scope.ServiceProvider
            .GetRequiredService<IOptions<IdentityOptions>>()
            .Value;

        Assert.Equal(12, identity.Password.RequiredLength);
        Assert.Equal(5, identity.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(
            TimeSpan.FromMinutes(30),
            identity.Lockout.DefaultLockoutTimeSpan);

        var cookieOptions = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);

        Assert.Equal(
            "__Host-ConstructionManagement.Auth",
            cookieOptions.Cookie.Name);
        Assert.Equal(
            Microsoft.AspNetCore.Http.CookieSecurePolicy.Always,
            cookieOptions.Cookie.SecurePolicy);
        Assert.True(cookieOptions.Cookie.HttpOnly);
        Assert.False(cookieOptions.SlidingExpiration);
    }

    [Fact]
    public void Security_event_storage_has_expected_indexes_and_no_identity_foreign_key()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(SecurityEventLog));

        Assert.NotNull(entity);
        Assert.Equal("SecurityEvents", entity.GetTableName());
        Assert.Empty(entity.GetForeignKeys());

        Assert.Contains(
            entity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    [
                        nameof(SecurityEventLog.EventType),
                        nameof(SecurityEventLog.OccurredAt)
                    ]));
    }

    [Fact]
    public async Task Login_response_contains_security_headers()
    {
        var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

        using var response = await client.GetAsync(
            "/login",
            TestContext.Current.CancellationToken);

        Assert.True(response.Headers.Contains(
            "Content-Security-Policy"));
        Assert.Equal(
            "nosniff",
            response.Headers.GetValues(
                "X-Content-Type-Options").Single());
        Assert.Equal(
            "DENY",
            response.Headers.GetValues(
                "X-Frame-Options").Single());
        Assert.Equal(
            "no-referrer",
            response.Headers.GetValues(
                "Referrer-Policy").Single());
    }
}
