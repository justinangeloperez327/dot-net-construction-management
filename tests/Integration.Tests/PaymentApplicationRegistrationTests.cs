using Application.PaymentApplications;
using Domain.PaymentApplications;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class PaymentApplicationRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentApplicationRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Payment_application_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IPaymentApplicationRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreatePaymentApplicationHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<SubmitPaymentApplicationHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<GetPaymentApplicationHandler>());
    }

    [Fact]
    public void Application_number_is_unique_within_purchase_order()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(PaymentApplication));

        Assert.NotNull(entity);
        Assert.Equal("PaymentApplications", entity.GetTableName());

        var index = entity.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(PaymentApplication.PurchaseOrderId),
                            nameof(PaymentApplication.ApplicationNumber)
                        ]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Claim_numbers_use_purchase_order_precision()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var entity = dbContext.Model.FindEntityType(
            typeof(PaymentApplicationItem));

        Assert.NotNull(entity);

        var quantity = entity.FindProperty(
            nameof(PaymentApplicationItem.ClaimedQuantity));
        var unitPrice = entity.FindProperty(
            nameof(PaymentApplicationItem.UnitPrice));
        var discount = entity.FindProperty(
            nameof(PaymentApplicationItem.DiscountPercent));
        var tax = entity.FindProperty(
            nameof(PaymentApplicationItem.TaxPercent));

        Assert.Equal(18, quantity!.GetPrecision());
        Assert.Equal(3, quantity.GetScale());

        Assert.Equal(18, unitPrice!.GetPrecision());
        Assert.Equal(4, unitPrice.GetScale());

        Assert.Equal(5, discount!.GetPrecision());
        Assert.Equal(2, discount.GetScale());

        Assert.Equal(5, tax!.GetPrecision());
        Assert.Equal(2, tax.GetScale());
    }
}
