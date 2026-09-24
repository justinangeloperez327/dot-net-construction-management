namespace Domain.DailyReports;

public sealed class DailyReportEquipmentEntry
{
    private DailyReportEquipmentEntry()
    {
    }

    internal DailyReportEquipmentEntry(
        Guid dailyReportId,
        string equipment,
        string? identifier,
        int quantity,
        decimal? hoursUsed,
        string? remarks)
    {
        Id = Guid.NewGuid();
        DailyReportId = dailyReportId;

        Update(
            equipment,
            identifier,
            quantity,
            hoursUsed,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid DailyReportId { get; private set; }

    public string Equipment { get; private set; } = string.Empty;

    public string? Identifier { get; private set; }

    public int Quantity { get; private set; }

    public decimal? HoursUsed { get; private set; }

    public string? Remarks { get; private set; }

    internal void Update(
        string equipment,
        string? identifier,
        int quantity,
        decimal? hoursUsed,
        string? remarks)
    {
        equipment = equipment.Trim();

        if (string.IsNullOrWhiteSpace(equipment))
        {
            throw new ArgumentException(
                "Equipment name is required.",
                nameof(equipment));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        if (hoursUsed is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(hoursUsed),
                "Hours used cannot be negative.");
        }

        Equipment = equipment;
        Identifier = NormalizeOptional(identifier);
        Quantity = quantity;
        HoursUsed = hoursUsed;
        Remarks = NormalizeOptional(remarks);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
