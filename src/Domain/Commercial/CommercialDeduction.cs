namespace Domain.Commercial;

public sealed class CommercialDeduction
{
    private CommercialDeduction()
    {
    }

    internal CommercialDeduction(
        Guid commercialCertificationId,
        string description,
        decimal amount)
    {
        Id = Guid.NewGuid();
        CommercialCertificationId = commercialCertificationId;
        SetDetails(description, amount);
    }

    public Guid Id { get; private set; }

    public Guid CommercialCertificationId { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    internal void Update(string description, decimal amount) =>
        SetDetails(description, amount);

    private void SetDetails(string description, decimal amount)
    {
        description = description.Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Deduction description is required.",
                nameof(description));
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Deduction amount cannot be negative.");
        }

        Description = description;
        Amount = decimal.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);
    }
}
