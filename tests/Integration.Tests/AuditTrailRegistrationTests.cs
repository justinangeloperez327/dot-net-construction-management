using Application.Auditing;
using Application.Common.Authorization;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Auditing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class AuditTrailRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuditTrailRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Audit_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IAuditTrailQueryService>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ListAuditTrailHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<ListAuditEntityTypesHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<AuditSaveChangesInterceptor>());
    }

    [Fact]
    public void Audit_permission_is_in_permission_catalog()
    {
        Assert.Contains(
            Permissions.AuditTrail.View,
            Permissions.All);
    }

    [Fact]
    public void Audit_log_model_is_append_only_storage_without_business_foreign_keys()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(AuditLog));

        Assert.NotNull(entity);
        Assert.Equal("AuditLogs", entity.GetTableName());
        Assert.Empty(entity.GetForeignKeys());

        Assert.Contains(
            entity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    [
                        nameof(AuditLog.ProjectId),
                        nameof(AuditLog.OccurredAt)
                    ]));

        Assert.Contains(
            entity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    [
                        nameof(AuditLog.EntityType),
                        nameof(AuditLog.EntityId)
                    ]));
    }
}
