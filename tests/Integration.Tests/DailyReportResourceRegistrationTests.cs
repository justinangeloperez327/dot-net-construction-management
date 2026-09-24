using Application.DailyReports;
using Domain.DailyReports;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class DailyReportResourceRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DailyReportResourceRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Resource_handlers_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<AddManpowerEntryHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<AddEquipmentEntryHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<AddSiteIssueHandler>());
    }

    [Fact]
    public void Resource_entities_are_mapped()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        Assert.Equal(
            "DailyReportManpower",
            dbContext.Model.FindEntityType(
                typeof(DailyReportManpowerEntry))?.GetTableName());

        Assert.Equal(
            "DailyReportEquipment",
            dbContext.Model.FindEntityType(
                typeof(DailyReportEquipmentEntry))?.GetTableName());

        Assert.Equal(
            "DailyReportSiteIssues",
            dbContext.Model.FindEntityType(
                typeof(DailyReportSiteIssue))?.GetTableName());
    }
}
