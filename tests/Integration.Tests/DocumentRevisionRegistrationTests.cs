using Application.Documents;
using Domain.Documents;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class DocumentRevisionRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentRevisionRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Revision_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateDocumentRevisionHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<SubmitDocumentRevisionHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ReviewDocumentRevisionHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetDocumentRevisionFileHandler>());
    }

    [Fact]
    public void Revision_mapping_has_document_scoped_unique_code()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(DocumentRevision));

        Assert.NotNull(entity);
        Assert.Equal(
            "DocumentRevisions",
            entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(DocumentRevision.DocumentId),
                            nameof(DocumentRevision.RevisionCode)
                        ]));

        Assert.True(index.IsUnique);
    }
}
