using Application.Clients;
using Application.Projects;
using Application.Users;
using Domain.Clients;
using Domain.Projects;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class ClientProjectMemberRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ClientProjectMemberRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Client_and_member_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IClientRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IProjectMemberRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<IUserDirectory>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateClientHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<AddProjectMemberHandler>());
    }

    [Fact]
    public void Client_and_project_member_are_mapped()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var clientEntity = dbContext.Model.FindEntityType(
            typeof(Client));

        var memberEntity = dbContext.Model.FindEntityType(
            typeof(ProjectMember));

        Assert.NotNull(clientEntity);
        Assert.NotNull(memberEntity);
        Assert.Equal("Clients", clientEntity.GetTableName());
        Assert.Equal("ProjectMembers", memberEntity.GetTableName());
    }

    [Fact]
    public void Project_has_optional_client_foreign_key()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var projectEntity = dbContext.Model.FindEntityType(
            typeof(Project));

        Assert.NotNull(projectEntity);

        var clientForeignKey = projectEntity
            .GetForeignKeys()
            .Single(foreignKey =>
                foreignKey.Properties.Any(
                    property => property.Name == nameof(Project.ClientId)));

        Assert.False(clientForeignKey.IsRequired);
    }
}
