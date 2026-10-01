using Application.Common.Authorization;
using Application.Reporting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class ReportingRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReportingRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Reporting_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IReportingQueryService>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetPortfolioReportHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetDailySiteReportHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetProcurementReportHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetCommercialReportHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ExportReportCsvHandler>());
    }

    [Fact]
    public void Reporting_permissions_are_in_permission_catalog()
    {
        Assert.Contains(
            Permissions.Reports.View,
            Permissions.All);

        Assert.Contains(
            Permissions.Reports.Export,
            Permissions.All);
    }
}
