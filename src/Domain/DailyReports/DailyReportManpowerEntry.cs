namespace Domain.DailyReports;

public sealed class DailyReportManpowerEntry
{
    private DailyReportManpowerEntry()
    {
    }

    internal DailyReportManpowerEntry(
        Guid dailyReportId,
        string trade,
        string? contractor,
        int headcount,
        decimal? manHours,
        string? remarks)
    {
        Id = Guid.NewGuid();
        DailyReportId = dailyReportId;

        Update(
            trade,
            contractor,
            headcount,
            manHours,
            remarks);
    }

    public Guid Id { get; private set; }

    public Guid DailyReportId { get; private set; }

    public string Trade { get; private set; } = string.Empty;

    public string? Contractor { get; private set; }

    public int Headcount { get; private set; }

    public decimal? ManHours { get; private set; }

    public string? Remarks { get; private set; }

    internal void Update(
        string trade,
        string? contractor,
        int headcount,
        decimal? manHours,
        string? remarks)
    {
        trade = trade.Trim();

        if (string.IsNullOrWhiteSpace(trade))
        {
            throw new ArgumentException(
                "Trade or discipline is required.",
                nameof(trade));
        }

        if (headcount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(headcount),
                "Headcount must be greater than zero.");
        }

        if (manHours is < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(manHours),
                "Man-hours cannot be negative.");
        }

        Trade = trade;
        Contractor = NormalizeOptional(contractor);
        Headcount = headcount;
        ManHours = manHours;
        Remarks = NormalizeOptional(remarks);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
