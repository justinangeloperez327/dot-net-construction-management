using Application.Commercial;
using Domain.Commercial;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class CommercialCertificationRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CommercialCertificationRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Commercial_certification_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<ICommercialCertificationRepository>());
        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateCommercialCertificationHandler>());
        Assert.NotNull(
            scope.ServiceProvider.GetService<SubmitCommercialCertificationHandler>());
        Assert.NotNull(
            scope.ServiceProvider.GetService<GetCommercialCertificationHandler>());
    }

    [Fact]
    public void Certification_is_unique_per_payment_application()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(CommercialCertification));

        Assert.NotNull(entity);

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [nameof(CommercialCertification.PaymentApplicationId)]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Certificate_number_is_unique_within_project()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(CommercialCertification));

        var index = entity!.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(CommercialCertification.ProjectId),
                            nameof(CommercialCertification.CertificateNumber)
                        ]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Commercial_amounts_use_fixed_precision()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(CommercialCertification));

        Assert.Equal(
            2,
            entity!.FindProperty(
                nameof(CommercialCertification.CertifiedAmount))!.GetScale());

        Assert.Equal(
            2,
            entity.FindProperty(
                nameof(CommercialCertification.RetentionPercent))!.GetScale());
    }
}
