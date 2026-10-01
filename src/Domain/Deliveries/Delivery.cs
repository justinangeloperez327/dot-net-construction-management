namespace Domain.Deliveries;

public sealed class Delivery
{
    private readonly List<DeliveryItem> _items = [];

    private Delivery()
    {
    }

    private Delivery(
        Guid projectId,
        Guid purchaseOrderId,
        string deliveryNoteNumber,
        DateOnly deliveryDate,
        string? vehicleReference,
        string? remarks,
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
        Status = DeliveryStatus.Draft;

        SetHeader(
            deliveryNoteNumber,
            deliveryDate,
            vehicleReference,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid PurchaseOrderId { get; private set; }

    public string DeliveryNoteNumber { get; private set; } = string.Empty;

    public DateOnly DeliveryDate { get; private set; }

    public string? VehicleReference { get; private set; }

    public string? Remarks { get; private set; }

    public DeliveryStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? ReceivedByUserId { get; private set; }

    public DateTimeOffset? ReceivedAt { get; private set; }

    public IReadOnlyCollection<DeliveryItem> Items => _items;

    public static Delivery Create(
        Guid projectId,
        Guid purchaseOrderId,
        string deliveryNoteNumber,
        DateOnly deliveryDate,
        string? vehicleReference,
        string? remarks,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        return new Delivery(
            projectId,
            purchaseOrderId,
            deliveryNoteNumber,
            deliveryDate,
            vehicleReference,
            remarks,
            createdByUserId,
            createdAt);
    }

    public void UpdateHeader(
        string deliveryNoteNumber,
        DateOnly deliveryDate,
        string? vehicleReference,
        string? remarks)
    {
        EnsureDraft();

        SetHeader(
            deliveryNoteNumber,
            deliveryDate,
            vehicleReference,
            remarks);
    }

    public DeliveryItem AddItem(
        Guid purchaseOrderItemId,
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        EnsureDraft();

        if (_items.Any(item =>
                item.PurchaseOrderItemId == purchaseOrderItemId))
        {
            throw new InvalidOperationException(
                "This purchase order item is already included in the delivery.");
        }

        var item = new DeliveryItem(
            Id,
            purchaseOrderItemId,
            description,
            quantity,
            unit,
            remarks);

        _items.Add(item);

        return item;
    }

    public void UpdateItem(
        Guid itemId,
        decimal quantity,
        string? remarks)
    {
        EnsureDraft();

        FindItem(itemId).Update(
            quantity,
            remarks);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureDraft();
        _items.Remove(FindItem(itemId));
    }

    public void Receive(
        Guid receivedByUserId,
        DateTimeOffset receivedAt)
    {
        EnsureDraft();

        if (receivedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Received-by user ID is required.",
                nameof(receivedByUserId));
        }

        if (_items.Count == 0)
        {
            throw new InvalidOperationException(
                "A delivery must contain at least one item before it can be received.");
        }

        ReceivedByUserId = receivedByUserId;
        ReceivedAt = receivedAt;
        Status = DeliveryStatus.Received;
    }

    public void Cancel()
    {
        EnsureDraft();
        Status = DeliveryStatus.Cancelled;
    }

    private DeliveryItem FindItem(Guid itemId) =>
        _items.SingleOrDefault(item => item.Id == itemId)
        ?? throw new InvalidOperationException(
            "Delivery item was not found.");

    private void SetHeader(
        string deliveryNoteNumber,
        DateOnly deliveryDate,
        string? vehicleReference,
        string? remarks)
    {
        deliveryNoteNumber = deliveryNoteNumber.Trim();

        if (string.IsNullOrWhiteSpace(deliveryNoteNumber))
        {
            throw new ArgumentException(
                "Delivery note number is required.",
                nameof(deliveryNoteNumber));
        }

        DeliveryNoteNumber = deliveryNoteNumber;
        DeliveryDate = deliveryDate;
        VehicleReference = NormalizeOptional(vehicleReference);
        Remarks = NormalizeOptional(remarks);
    }

    private void EnsureDraft()
    {
        if (Status != DeliveryStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft deliveries can be changed.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
