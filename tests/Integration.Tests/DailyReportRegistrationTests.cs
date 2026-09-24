using Application.DailyReports;
using Domain.DailyReports;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class DailyReportRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DailyReportRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Daily_report_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IDailyReportRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateDailyReportHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ReviewDailyReportHandler>());
    }

    [Fact]
    public void Daily_report_and_activities_are_mapped()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var reportEntity = dbContext.Model.FindEntityType(
            typeof(DailyReport));

        var activityEntity = dbContext.Model.FindEntityType(
            typeof(DailyReportActivity));

        Assert.NotNull(reportEntity);
        Assert.NotNull(activityEntity);
        Assert.Equal("DailyReports", reportEntity.GetTableName());
        Assert.Equal("DailyReportActivities", activityEntity.GetTableName());
    }

    [Fact]
    public void Project_and_report_date_have_unique_index()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var reportEntity = dbContext.Model.FindEntityType(
            typeof(DailyReport));

        Assert.NotNull(reportEntity);

        var index = reportEntity
            .GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(DailyReport.ProjectId),
                            nameof(DailyReport.ReportDate)
                        ]));

        Assert.True(index.IsUnique);
    }
}
