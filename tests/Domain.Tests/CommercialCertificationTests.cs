using Domain.Commercial;
using Xunit;

namespace Domain.Tests;

public sealed class CommercialCertificationTests
{
    [Fact]
    public void Payable_amount_is_derived_from_certification_and_deductions()
    {
        var certification = CreateCertification(
            claimedAmount: 1000m,
            certifiedAmount: 900m,
            retentionPercent: 10m,
            advanceRecovery: 100m);

        certification.AddOtherDeduction(
            "Back charge",
            50m);

        Assert.Equal(90m, certification.RetentionAmount);
        Assert.Equal(50m, certification.OtherDeductionAmount);
        Assert.Equal(240m, certification.TotalDeductions);
        Assert.Equal(660m, certification.PayableAmount);
    }

    [Fact]
    public void Certified_amount_cannot_exceed_claimed_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateCertification(
                claimedAmount: 1000m,
                certifiedAmount: 1000.01m,
                retentionPercent: 0m,
                advanceRecovery: 0m));
    }

    [Fact]
    public void Total_deductions_cannot_exceed_certified_amount()
    {
        var certification = CreateCertification(
            claimedAmount: 1000m,
            certifiedAmount: 100m,
            retentionPercent: 50m,
            advanceRecovery: 25m);

        Assert.Throws<InvalidOperationException>(() =>
            certification.AddOtherDeduction(
                "Back charge",
                30m));

        Assert.Empty(certification.OtherDeductions);
        Assert.Equal(25m, certification.PayableAmount);
    }

    [Fact]
    public void Invalid_header_edit_does_not_mutate_certification()
    {
        var certification = CreateCertification(
            claimedAmount: 1000m,
            certifiedAmount: 500m,
            retentionPercent: 10m,
            advanceRecovery: 50m);

        certification.AddOtherDeduction(
            "Back charge",
            50m);

        Assert.Throws<InvalidOperationException>(() =>
            certification.UpdateHeader(
                "CERT-CHANGED",
                new DateOnly(2026, 10, 10),
                100m,
                50m,
                50m,
                "Should fail"));

        Assert.Equal("CERT-001", certification.CertificateNumber);
        Assert.Equal(500m, certification.CertifiedAmount);
        Assert.Equal(10m, certification.RetentionPercent);
        Assert.Equal(50m, certification.AdvancePaymentRecoveryAmount);
        Assert.Null(certification.Notes);
    }

    [Fact]
    public void Approved_certification_is_immutable()
    {
        var certification = CreateCertification(
            claimedAmount: 1000m,
            certifiedAmount: 900m,
            retentionPercent: 10m,
            advanceRecovery: 0m);

        var approvalId = Guid.NewGuid();
        certification.SubmitForApproval(approvalId);
        certification.ApplyApprovalOutcome(
            approvalId,
            CommercialCertificationStatus.Approved);

        Assert.Throws<InvalidOperationException>(() =>
            certification.AddOtherDeduction(
                "Late deduction",
                10m));
    }

    private static CommercialCertification CreateCertification(
        decimal claimedAmount,
        decimal certifiedAmount,
        decimal retentionPercent,
        decimal advanceRecovery) =>
        CommercialCertification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-001",
            new DateOnly(2026, 10, 5),
            claimedAmount,
            certifiedAmount,
            retentionPercent,
            advanceRecovery,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
