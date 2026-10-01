using Domain.PaymentApplications;
using Xunit;

namespace Domain.Tests;

public sealed class PaymentApplicationTests
{
    [Fact]
    public void Claim_totals_are_derived_from_po_commercial_terms()
    {
        var application = CreateApplication();

        var item = application.AddItem(
            Guid.NewGuid(),
            "Cement board",
            2m,
            "pcs",
            10m,
            10m,
            5m,
            null);

        Assert.Equal(20m, item.GrossAmount);
        Assert.Equal(2m, item.DiscountAmount);
        Assert.Equal(0.90m, item.TaxAmount);
        Assert.Equal(18.90m, item.TotalAmount);
        Assert.Equal(18.90m, application.ClaimedAmount);
    }

    [Fact]
    public void Submit_requires_at_least_one_item()
    {
        var application = CreateApplication();

        Assert.Throws<InvalidOperationException>(() =>
            application.SubmitForApproval(Guid.NewGuid()));
    }

    [Fact]
    public void Rejected_application_can_be_corrected_and_resubmitted()
    {
        var application = CreateApplication();

        var item = application.AddItem(
            Guid.NewGuid(),
            "Cement board",
            2m,
            "pcs",
            10m,
            0m,
            0m,
            null);

        var firstApprovalId = Guid.NewGuid();
        application.SubmitForApproval(firstApprovalId);
        application.ApplyApprovalOutcome(
            firstApprovalId,
            PaymentApplicationStatus.Rejected);

        application.UpdateItem(
            item.Id,
            1m,
            "Revised claim.");

        var secondApprovalId = Guid.NewGuid();
        application.SubmitForApproval(secondApprovalId);

        Assert.Equal(
            PaymentApplicationStatus.PendingApproval,
            application.Status);
        Assert.Equal(
            secondApprovalId,
            application.ApprovalRequestId);
    }

    [Fact]
    public void Period_end_cannot_precede_period_start()
    {
        Assert.Throws<ArgumentException>(() =>
            PaymentApplication.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "PA-001",
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 9, 30),
                null,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));
    }

    private static PaymentApplication CreateApplication() =>
        PaymentApplication.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PA-001",
            new DateOnly(2026, 10, 1),
            null,
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
