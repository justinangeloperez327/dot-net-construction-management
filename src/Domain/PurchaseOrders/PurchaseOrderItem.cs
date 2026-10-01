namespace Domain.PurchaseOrders;

public sealed class PurchaseOrderItem
{
    private PurchaseOrderItem()
    {
    }

    internal PurchaseOrderItem(
        Guid purchaseOrderId,
        Guid? purchaseRequestItemId,
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        string? remarks)
    {
        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        PurchaseRequestItemId = purchaseRequestItemId;

        SetDetails(
            description,
            quantity,
            unit,
            unitPrice,
            discountPercent,
            taxPercent,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid PurchaseOrderId { get; private set; }

    public Guid? PurchaseRequestItemId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public decimal DiscountPercent { get; private set; }

    public decimal TaxPercent { get; private set; }

    public string? Remarks { get; private set; }

    public decimal GrossAmount =>
        decimal.Round(
            Quantity * UnitPrice,
            2,
            MidpointRounding.AwayFromZero);

    public decimal DiscountAmount =>
        decimal.Round(
            GrossAmount * DiscountPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);

    public decimal NetAmount => GrossAmount - DiscountAmount;

    public decimal TaxAmount =>
        decimal.Round(
            NetAmount * TaxPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);

    public decimal TotalAmount => NetAmount + TaxAmount;

    internal void Update(
        decimal quantity,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        string? remarks)
    {
        SetDetails(
            Description,
            quantity,
            Unit,
            unitPrice,
            discountPercent,
            taxPercent,
            remarks);
    }

    private void SetDetails(
        string description,
        decimal quantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
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
                "Item quantity must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException(
                "Item unit is required.",
                nameof(unit));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice),
                "Unit price cannot be negative.");
        }

        ValidatePercent(discountPercent, nameof(discountPercent));
        ValidatePercent(taxPercent, nameof(taxPercent));

        Description = description;
        Quantity = quantity;
        Unit = unit;
        UnitPrice = unitPrice;
        DiscountPercent = discountPercent;
        TaxPercent = taxPercent;
        Remarks = NormalizeOptional(remarks);
    }

    private static void ValidatePercent(
        decimal value,
        string parameterName)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Percentage must be between 0 and 100.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
