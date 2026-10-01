namespace Domain.PurchaseOrders;

public sealed class PurchaseOrder
{
    private readonly List<PurchaseOrderItem> _items = [];

    private PurchaseOrder()
    {
    }

    private PurchaseOrder(
        Guid projectId,
        Guid purchaseRequestId,
        Guid supplierId,
        string purchaseOrderNumber,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string currency,
        string? deliveryAddress,
        string? deliveryTerms,
        string? paymentTerms,
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

        if (purchaseRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Purchase request ID is required.",
                nameof(purchaseRequestId));
        }

        if (supplierId == Guid.Empty)
        {
            throw new ArgumentException(
                "Supplier ID is required.",
                nameof(supplierId));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Created-by user ID is required.",
                nameof(createdByUserId));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        PurchaseRequestId = purchaseRequestId;
        SupplierId = supplierId;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        Status = PurchaseOrderStatus.Draft;

        SetHeader(
            purchaseOrderNumber,
            orderDate,
            expectedDeliveryDate,
            currency,
            deliveryAddress,
            deliveryTerms,
            paymentTerms,
            notes);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid PurchaseRequestId { get; private set; }

    public Guid SupplierId { get; private set; }

    public string PurchaseOrderNumber { get; private set; } = string.Empty;

    public DateOnly OrderDate { get; private set; }

    public DateOnly? ExpectedDeliveryDate { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public string? DeliveryAddress { get; private set; }

    public string? DeliveryTerms { get; private set; }

    public string? PaymentTerms { get; private set; }

    public string? Notes { get; private set; }

    public PurchaseOrderStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? ApprovalRequestId { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items;

    public decimal Subtotal =>
        _items.Sum(item => item.GrossAmount);

    public decimal DiscountTotal =>
        _items.Sum(item => item.DiscountAmount);

    public decimal TaxTotal =>
        _items.Sum(item => item.TaxAmount);

    public decimal GrandTotal =>
        _items.Sum(item => item.TotalAmount);

    public static PurchaseOrder Create(
        Guid projectId,
        Guid purchaseRequestId,
        Guid supplierId,
        string purchaseOrderNumber,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string currency,
        string? deliveryAddress,
        string? deliveryTerms,
        string? paymentTerms,
        string? notes,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        return new PurchaseOrder(
            projectId,
            purchaseRequestId,
            supplierId,
            purchaseOrderNumber,
            orderDate,
            expectedDeliveryDate,
            currency,
            deliveryAddress,
            deliveryTerms,
            paymentTerms,
            notes,
            createdByUserId,
            createdAt);
    }

    public void UpdateHeader(
        Guid supplierId,
        string purchaseOrderNumber,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string currency,
        string? deliveryAddress,
        string? deliveryTerms,
        string? paymentTerms,
        string? notes)
    {
        EnsureEditable();

        if (supplierId == Guid.Empty)
        {
            throw new ArgumentException(
                "Supplier ID is required.",
                nameof(supplierId));
        }

        SupplierId = supplierId;

        SetHeader(
            purchaseOrderNumber,
            orderDate,
            expectedDeliveryDate,
            currency,
            deliveryAddress,
            deliveryTerms,
            paymentTerms,
            notes);
    }

    public PurchaseOrderItem AddItem(
        Guid? purchaseRequestItemId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        string? remarks)
    {
        EnsureEditable();

        if (purchaseRequestItemId is not null &&
            _items.Any(item =>
                item.PurchaseRequestItemId == purchaseRequestItemId))
        {
            throw new InvalidOperationException(
                "This purchase request item is already included in the purchase order.");
        }

        var item = new PurchaseOrderItem(
            Id,
            purchaseRequestItemId,
            description,
            quantity,
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
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        string? remarks)
    {
        EnsureEditable();

        FindItem(itemId).Update(
            quantity,
            unitPrice,
            discountPercent,
            taxPercent,
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
                "A purchase order must contain at least one item before submission.");
        }

        if (GrandTotal <= 0)
        {
            throw new InvalidOperationException(
                "Purchase order total must be greater than zero before submission.");
        }

        if (approvalRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Approval request ID is required.",
                nameof(approvalRequestId));
        }

        ApprovalRequestId = approvalRequestId;
        Status = PurchaseOrderStatus.PendingApproval;
    }

    public void ApplyApprovalOutcome(
        Guid approvalRequestId,
        PurchaseOrderStatus outcome)
    {
        if (Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only purchase orders pending approval can receive an approval outcome.");
        }

        if (ApprovalRequestId != approvalRequestId)
        {
            throw new InvalidOperationException(
                "Approval outcome does not match the current approval request.");
        }

        if (outcome == PurchaseOrderStatus.Approved)
        {
            Status = PurchaseOrderStatus.Approved;
            return;
        }

        if (outcome == PurchaseOrderStatus.Rejected)
        {
            Status = PurchaseOrderStatus.Rejected;
            return;
        }

        if (outcome == PurchaseOrderStatus.Draft)
        {
            Status = PurchaseOrderStatus.Draft;
            ApprovalRequestId = null;
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(outcome),
            "Unsupported purchase order approval outcome.");
    }

    public void Cancel()
    {
        if (Status is not PurchaseOrderStatus.Draft
            and not PurchaseOrderStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected purchase orders can be cancelled.");
        }

        Status = PurchaseOrderStatus.Cancelled;
    }

    private PurchaseOrderItem FindItem(Guid itemId) =>
        _items.SingleOrDefault(item => item.Id == itemId)
        ?? throw new InvalidOperationException(
            "Purchase order item was not found.");

    private void SetHeader(
        string purchaseOrderNumber,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string currency,
        string? deliveryAddress,
        string? deliveryTerms,
        string? paymentTerms,
        string? notes)
    {
        purchaseOrderNumber = purchaseOrderNumber.Trim();
        currency = currency.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(purchaseOrderNumber))
        {
            throw new ArgumentException(
                "Purchase order number is required.",
                nameof(purchaseOrderNumber));
        }

        if (currency.Length != 3 ||
            !currency.All(char.IsLetter))
        {
            throw new ArgumentException(
                "Currency must be a three-letter code.",
                nameof(currency));
        }

        if (expectedDeliveryDate is not null &&
            expectedDeliveryDate < orderDate)
        {
            throw new ArgumentException(
                "Expected delivery date cannot be before the order date.",
                nameof(expectedDeliveryDate));
        }

        PurchaseOrderNumber = purchaseOrderNumber;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Currency = currency;
        DeliveryAddress = NormalizeOptional(deliveryAddress);
        DeliveryTerms = NormalizeOptional(deliveryTerms);
        PaymentTerms = NormalizeOptional(paymentTerms);
        Notes = NormalizeOptional(notes);
    }

    private void EnsureEditable()
    {
        if (Status is not PurchaseOrderStatus.Draft
            and not PurchaseOrderStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected purchase orders can be edited.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
