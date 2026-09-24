using Application.Documents;
using Domain.Documents;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class DocumentRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Document_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IDocumentRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateDocumentHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<UpdateDocumentHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<SetDocumentArchivedHandler>());
    }

    [Fact]
    public void Document_mapping_has_project_scoped_unique_number()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(Document));

        Assert.NotNull(entity);
        Assert.Equal("Documents", entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(Document.ProjectId),
                            nameof(Document.DocumentNumber)
                        ]));

        Assert.True(index.IsUnique);
    }
}
