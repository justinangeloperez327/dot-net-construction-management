namespace Domain.PurchaseRequests;

public sealed class PurchaseRequestItem
{
    private PurchaseRequestItem()
    {
    }

    internal PurchaseRequestItem(
        Guid purchaseRequestId,
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        Id = Guid.NewGuid();
        PurchaseRequestId = purchaseRequestId;

        SetDetails(
            description,
            quantity,
            unit,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid PurchaseRequestId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string Unit { get; private set; } = string.Empty;

    public string? Remarks { get; private set; }

    internal void Update(
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        SetDetails(
            description,
            quantity,
            unit,
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
                "Item quantity must be greater than zero.");
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
