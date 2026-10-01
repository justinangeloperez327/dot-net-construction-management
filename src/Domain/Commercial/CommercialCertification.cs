namespace Domain.Commercial;

public sealed class CommercialCertification
{
    private readonly List<CommercialDeduction> _otherDeductions = [];

    private CommercialCertification()
    {
    }

    private CommercialCertification(
        Guid projectId,
        Guid paymentApplicationId,
        string certificateNumber,
        DateOnly certificateDate,
        decimal claimedAmount,
        decimal certifiedAmount,
        decimal retentionPercent,
        decimal advancePaymentRecoveryAmount,
        string? notes,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        if (paymentApplicationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Payment application ID is required.",
                nameof(paymentApplicationId));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Created-by user ID is required.",
                nameof(createdByUserId));
        }

        if (claimedAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(claimedAmount),
                "Claimed amount must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        PaymentApplicationId = paymentApplicationId;
        ClaimedAmount = decimal.Round(
            claimedAmount,
            2,
            MidpointRounding.AwayFromZero);
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = CommercialCertificationStatus.Draft;

        SetHeader(
            certificateNumber,
            certificateDate,
            certifiedAmount,
            retentionPercent,
            advancePaymentRecoveryAmount,
            notes);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid PaymentApplicationId { get; private set; }

    public string CertificateNumber { get; private set; } = string.Empty;

    public DateOnly CertificateDate { get; private set; }

    public decimal ClaimedAmount { get; private set; }

    public decimal CertifiedAmount { get; private set; }

    public decimal RetentionPercent { get; private set; }

    public decimal AdvancePaymentRecoveryAmount { get; private set; }

    public string? Notes { get; private set; }

    public CommercialCertificationStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? ApprovalRequestId { get; private set; }

    public IReadOnlyCollection<CommercialDeduction> OtherDeductions =>
        _otherDeductions;

    public decimal RetentionAmount =>
        decimal.Round(
            CertifiedAmount * RetentionPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);

    public decimal OtherDeductionAmount =>
        _otherDeductions.Sum(deduction => deduction.Amount);

    public decimal TotalDeductions =>
        RetentionAmount
        + AdvancePaymentRecoveryAmount
        + OtherDeductionAmount;

    public decimal PayableAmount =>
        CertifiedAmount - TotalDeductions;

    public static CommercialCertification Create(
        Guid projectId,
        Guid paymentApplicationId,
        string certificateNumber,
        DateOnly certificateDate,
        decimal claimedAmount,
        decimal certifiedAmount,
        decimal retentionPercent,
        decimal advancePaymentRecoveryAmount,
        string? notes,
        Guid createdByUserId,
        DateTimeOffset createdAt) =>
        new(
            projectId,
            paymentApplicationId,
            certificateNumber,
            certificateDate,
            claimedAmount,
            certifiedAmount,
            retentionPercent,
            advancePaymentRecoveryAmount,
            notes,
            createdByUserId,
            createdAt);

    public void UpdateHeader(
        string certificateNumber,
        DateOnly certificateDate,
        decimal certifiedAmount,
        decimal retentionPercent,
        decimal advancePaymentRecoveryAmount,
        string? notes)
    {
        EnsureEditable();

        SetHeader(
            certificateNumber,
            certificateDate,
            certifiedAmount,
            retentionPercent,
            advancePaymentRecoveryAmount,
            notes);
    }

    public CommercialDeduction AddOtherDeduction(
        string description,
        decimal amount)
    {
        EnsureEditable();

        var deduction = new CommercialDeduction(
            Id,
            description,
            amount);

        EnsureDeductionsWithinCertifiedAmount(
            additionalAmount: deduction.Amount);

        _otherDeductions.Add(deduction);

        return deduction;
    }

    public void UpdateOtherDeduction(
        Guid deductionId,
        string description,
        decimal amount)
    {
        EnsureEditable();

        var deduction = FindDeduction(deductionId);
        var originalDescription = deduction.Description;
        var originalAmount = deduction.Amount;

        deduction.Update(description, amount);

        try
        {
            EnsureDeductionsWithinCertifiedAmount();
        }
        catch
        {
            deduction.Update(originalDescription, originalAmount);
            throw;
        }
    }

    public void RemoveOtherDeduction(Guid deductionId)
    {
        EnsureEditable();
        _otherDeductions.Remove(FindDeduction(deductionId));
    }

    public void SubmitForApproval(Guid approvalRequestId)
    {
        EnsureEditable();

        if (CertifiedAmount <= 0)
        {
            throw new InvalidOperationException(
                "Certified amount must be greater than zero before submission.");
        }

        EnsureDeductionsWithinCertifiedAmount();

        if (approvalRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Approval request ID is required.",
                nameof(approvalRequestId));
        }

        ApprovalRequestId = approvalRequestId;
        Status = CommercialCertificationStatus.PendingApproval;
    }

    public void ApplyApprovalOutcome(
        Guid approvalRequestId,
        CommercialCertificationStatus outcome)
    {
        if (Status != CommercialCertificationStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only commercial certifications pending approval can receive an approval outcome.");
        }

        if (ApprovalRequestId != approvalRequestId)
        {
            throw new InvalidOperationException(
                "Approval outcome does not match the current approval request.");
        }

        if (outcome == CommercialCertificationStatus.Approved)
        {
            Status = CommercialCertificationStatus.Approved;
            return;
        }

        if (outcome == CommercialCertificationStatus.Rejected)
        {
            Status = CommercialCertificationStatus.Rejected;
            return;
        }

        if (outcome == CommercialCertificationStatus.Draft)
        {
            Status = CommercialCertificationStatus.Draft;
            ApprovalRequestId = null;
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(outcome),
            "Unsupported commercial certification approval outcome.");
    }

    public void Cancel()
    {
        if (Status is not CommercialCertificationStatus.Draft
            and not CommercialCertificationStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected commercial certifications can be cancelled.");
        }

        Status = CommercialCertificationStatus.Cancelled;
    }

    private CommercialDeduction FindDeduction(Guid deductionId) =>
        _otherDeductions.SingleOrDefault(
            deduction => deduction.Id == deductionId)
        ?? throw new InvalidOperationException(
            "Commercial deduction was not found.");

    private void SetHeader(
        string certificateNumber,
        DateOnly certificateDate,
        decimal certifiedAmount,
        decimal retentionPercent,
        decimal advancePaymentRecoveryAmount,
        string? notes)
    {
        certificateNumber = certificateNumber.Trim();

        if (string.IsNullOrWhiteSpace(certificateNumber))
        {
            throw new ArgumentException(
                "Certificate number is required.",
                nameof(certificateNumber));
        }

        if (certifiedAmount < 0 || certifiedAmount > ClaimedAmount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(certifiedAmount),
                "Certified amount must be between zero and the claimed amount.");
        }

        if (retentionPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retentionPercent),
                "Retention percentage must be between 0 and 100.");
        }

        if (advancePaymentRecoveryAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(advancePaymentRecoveryAmount),
                "Advance payment recovery cannot be negative.");
        }

        var roundedCertifiedAmount = decimal.Round(
            certifiedAmount,
            2,
            MidpointRounding.AwayFromZero);
        var roundedRetentionPercent = decimal.Round(
            retentionPercent,
            2,
            MidpointRounding.AwayFromZero);
        var roundedAdvanceRecovery = decimal.Round(
            advancePaymentRecoveryAmount,
            2,
            MidpointRounding.AwayFromZero);
        var proposedRetentionAmount = decimal.Round(
            roundedCertifiedAmount * roundedRetentionPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);
        var proposedTotalDeductions =
            proposedRetentionAmount
            + roundedAdvanceRecovery
            + OtherDeductionAmount;

        if (proposedTotalDeductions > roundedCertifiedAmount)
        {
            throw new InvalidOperationException(
                "Total deductions cannot exceed the certified amount.");
        }

        CertificateNumber = certificateNumber;
        CertificateDate = certificateDate;
        CertifiedAmount = roundedCertifiedAmount;
        RetentionPercent = roundedRetentionPercent;
        AdvancePaymentRecoveryAmount = roundedAdvanceRecovery;
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    private void EnsureDeductionsWithinCertifiedAmount(
        decimal additionalAmount = 0)
    {
        var total =
            RetentionAmount
            + AdvancePaymentRecoveryAmount
            + OtherDeductionAmount
            + additionalAmount;

        if (total > CertifiedAmount)
        {
            throw new InvalidOperationException(
                "Total deductions cannot exceed the certified amount.");
        }
    }

    private void EnsureEditable()
    {
        if (Status is not CommercialCertificationStatus.Draft
            and not CommercialCertificationStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected commercial certifications can be edited.");
        }
    }
}
