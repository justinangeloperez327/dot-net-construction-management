using Application.Projects;
using Domain.Projects;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class ProjectRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProjectRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Project_repository_and_handlers_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IProjectRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateProjectHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ListProjectsHandler>());
    }

    [Fact]
    public void Project_is_mapped_to_projects_table()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entityType = dbContext.Model.FindEntityType(
            typeof(Project));

        Assert.NotNull(entityType);
        Assert.Equal("Projects", entityType.GetTableName());
    }
}
