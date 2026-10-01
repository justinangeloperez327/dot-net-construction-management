namespace Domain.PaymentApplications;

public sealed class PaymentApplication
{
    private readonly List<PaymentApplicationItem> _items = [];

    private PaymentApplication()
    {
    }

    private PaymentApplication(
        Guid projectId,
        Guid purchaseOrderId,
        string applicationNumber,
        DateOnly applicationDate,
        DateOnly? periodFrom,
        DateOnly? periodTo,
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

        if (purchaseOrderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Purchase order ID is required.",
                nameof(purchaseOrderId));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Created-by user ID is required.",
                nameof(createdByUserId));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        PurchaseOrderId = purchaseOrderId;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = PaymentApplicationStatus.Draft;

        SetHeader(
            applicationNumber,
            applicationDate,
            periodFrom,
            periodTo,
            notes);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid PurchaseOrderId { get; private set; }

    public string ApplicationNumber { get; private set; } = string.Empty;

    public DateOnly ApplicationDate { get; private set; }

    public DateOnly? PeriodFrom { get; private set; }

    public DateOnly? PeriodTo { get; private set; }

    public string? Notes { get; private set; }

    public PaymentApplicationStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? ApprovalRequestId { get; private set; }

    public IReadOnlyCollection<PaymentApplicationItem> Items => _items;

    public decimal GrossAmount =>
        _items.Sum(item => item.GrossAmount);

    public decimal DiscountAmount =>
        _items.Sum(item => item.DiscountAmount);

    public decimal TaxAmount =>
        _items.Sum(item => item.TaxAmount);

    public decimal ClaimedAmount =>
        _items.Sum(item => item.TotalAmount);

    public static PaymentApplication Create(
        Guid projectId,
        Guid purchaseOrderId,
        string applicationNumber,
        DateOnly applicationDate,
        DateOnly? periodFrom,
        DateOnly? periodTo,
        string? notes,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        return new PaymentApplication(
            projectId,
            purchaseOrderId,
            applicationNumber,
            applicationDate,
            periodFrom,
            periodTo,
            notes,
            createdByUserId,
            createdAt);
    }

    public void UpdateHeader(
        string applicationNumber,
        DateOnly applicationDate,
        DateOnly? periodFrom,
        DateOnly? periodTo,
        string? notes)
    {
        EnsureEditable();

        SetHeader(
            applicationNumber,
            applicationDate,
            periodFrom,
            periodTo,
            notes);
    }

    public PaymentApplicationItem AddItem(
        Guid purchaseOrderItemId,
        string description,
        decimal claimedQuantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        string? remarks)
    {
        EnsureEditable();

        if (_items.Any(item =>
                item.PurchaseOrderItemId == purchaseOrderItemId))
        {
            throw new InvalidOperationException(
                "This purchase order item is already included in the payment application.");
        }

        var item = new PaymentApplicationItem(
            Id,
            purchaseOrderItemId,
            description,
            claimedQuantity,
            unit,
            unitPrice,
            discountPercent,
            taxPercent,
            remarks);

        _items.Add(item);

        return item;
    }

    public void UpdateItem(
        Guid itemId,
        decimal claimedQuantity,
        string? remarks)
    {
        EnsureEditable();

        FindItem(itemId).Update(
            claimedQuantity,
            remarks);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        _items.Remove(FindItem(itemId));
    }

    public void SubmitForApproval(Guid approvalRequestId)
    {
        EnsureEditable();

        if (_items.Count == 0)
        {
            throw new InvalidOperationException(
                "A payment application must contain at least one item before submission.");
        }

        if (ClaimedAmount <= 0)
        {
            throw new InvalidOperationException(
                "Claimed amount must be greater than zero before submission.");
        }

        if (approvalRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Approval request ID is required.",
                nameof(approvalRequestId));
        }

        ApprovalRequestId = approvalRequestId;
        Status = PaymentApplicationStatus.PendingApproval;
    }

    public void ApplyApprovalOutcome(
        Guid approvalRequestId,
        PaymentApplicationStatus outcome)
    {
        if (Status != PaymentApplicationStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only payment applications pending approval can receive an approval outcome.");
        }

        if (ApprovalRequestId != approvalRequestId)
        {
            throw new InvalidOperationException(
                "Approval outcome does not match the current approval request.");
        }

        if (outcome == PaymentApplicationStatus.Approved)
        {
            Status = PaymentApplicationStatus.Approved;
            return;
        }

        if (outcome == PaymentApplicationStatus.Rejected)
        {
            Status = PaymentApplicationStatus.Rejected;
            return;
        }

        if (outcome == PaymentApplicationStatus.Draft)
        {
            Status = PaymentApplicationStatus.Draft;
            ApprovalRequestId = null;
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(outcome),
            "Unsupported payment application approval outcome.");
    }

    public void Cancel()
    {
        if (Status is not PaymentApplicationStatus.Draft
            and not PaymentApplicationStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected payment applications can be cancelled.");
        }

        Status = PaymentApplicationStatus.Cancelled;
    }

    private PaymentApplicationItem FindItem(Guid itemId) =>
        _items.SingleOrDefault(item => item.Id == itemId)
        ?? throw new InvalidOperationException(
            "Payment application item was not found.");

    private void SetHeader(
        string applicationNumber,
        DateOnly applicationDate,
        DateOnly? periodFrom,
        DateOnly? periodTo,
        string? notes)
    {
        applicationNumber = applicationNumber.Trim();

        if (string.IsNullOrWhiteSpace(applicationNumber))
        {
            throw new ArgumentException(
                "Payment application number is required.",
                nameof(applicationNumber));
        }

        if (periodFrom is not null &&
            periodTo is not null &&
            periodTo < periodFrom)
        {
            throw new ArgumentException(
                "Payment application period end cannot be before its start.",
                nameof(periodTo));
        }

        ApplicationNumber = applicationNumber;
        ApplicationDate = applicationDate;
        PeriodFrom = periodFrom;
        PeriodTo = periodTo;
        Notes = NormalizeOptional(notes);
    }

    private void EnsureEditable()
    {
        if (Status is not PaymentApplicationStatus.Draft
            and not PaymentApplicationStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected payment applications can be edited.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
