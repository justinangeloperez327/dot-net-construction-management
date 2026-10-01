using Application.Common.Authorization;
using Application.Dashboard;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class DashboardRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DashboardRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Dashboard_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IDashboardQueryService>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetPortfolioDashboardHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetProjectDashboardHandler>());
    }

    [Fact]
    public void Dashboard_permission_is_part_of_permission_catalog()
    {
        Assert.Contains(
            Permissions.Dashboard.View,
            Permissions.All);
    }
}
