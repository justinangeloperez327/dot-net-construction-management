namespace Domain.Deliveries;

public sealed class DeliveryItem
{
    private DeliveryItem()
    {
    }

    internal DeliveryItem(
        Guid deliveryId,
        Guid purchaseOrderItemId,
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        if (purchaseOrderItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "Purchase order item ID is required.",
                nameof(purchaseOrderItemId));
        }

        Id = Guid.NewGuid();
        DeliveryId = deliveryId;
        PurchaseOrderItemId = purchaseOrderItemId;

        SetDetails(
            description,
            quantity,
            unit,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid DeliveryId { get; private set; }

    public Guid PurchaseOrderItemId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string? Remarks { get; private set; }

    internal void Update(
        decimal quantity,
        string? remarks)
    {
        SetDetails(
            Description,
            quantity,
            Unit,
            remarks);
    }

    private void SetDetails(
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        description = description.Trim();
        unit = unit.Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Item description is required.",
                nameof(description));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Delivered quantity must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException(
                "Item unit is required.",
                nameof(unit));
        }

        Description = description;
        Quantity = quantity;
        Unit = unit;
        Remarks = NormalizeOptional(remarks);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
