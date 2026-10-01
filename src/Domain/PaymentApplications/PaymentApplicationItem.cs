namespace Domain.PaymentApplications;

public sealed class PaymentApplicationItem
{
    private PaymentApplicationItem()
    {
    }

    internal PaymentApplicationItem(
        Guid paymentApplicationId,
        Guid purchaseOrderItemId,
        string description,
        decimal claimedQuantity,
        string unit,
        decimal unitPrice,
        decimal discountPercent,
        decimal taxPercent,
        string? remarks)
    {
        if (purchaseOrderItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "Purchase order item ID is required.",
                nameof(purchaseOrderItemId));
        }

        Id = Guid.NewGuid();
        PaymentApplicationId = paymentApplicationId;
        PurchaseOrderItemId = purchaseOrderItemId;

        SetDetails(
            description,
            claimedQuantity,
            unit,
            unitPrice,
            discountPercent,
            taxPercent,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid PaymentApplicationId { get; private set; }

    public Guid PurchaseOrderItemId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal ClaimedQuantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public decimal DiscountPercent { get; private set; }

    public decimal TaxPercent { get; private set; }

    public string? Remarks { get; private set; }

    public decimal GrossAmount =>
        decimal.Round(
            ClaimedQuantity * UnitPrice,
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
        decimal claimedQuantity,
        string? remarks)
    {
        SetDetails(
            Description,
            claimedQuantity,
            Unit,
            UnitPrice,
            DiscountPercent,
            TaxPercent,
            remarks);
    }

    private void SetDetails(
        string description,
        decimal claimedQuantity,
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

        if (claimedQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(claimedQuantity),
                "Claimed quantity must be greater than zero.");
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
        ClaimedQuantity = claimedQuantity;
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
