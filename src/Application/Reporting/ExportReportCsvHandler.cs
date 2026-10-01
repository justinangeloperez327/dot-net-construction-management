using System.Globalization;
using System.Text;

namespace Application.Reporting;

public sealed class ExportReportCsvHandler(
    IReportingQueryService reporting)
{
    private static readonly UTF8Encoding Utf8WithBom =
        new(encoderShouldEmitUTF8Identifier: true);

    public async Task<ReportCsvDocument> HandleAsync(
        ReportKind report,
        ReportingFilter filter,
        CancellationToken cancellationToken = default)
    {
        return report switch
        {
            ReportKind.Portfolio => BuildPortfolio(
                await reporting.GetPortfolioAsync(
                    filter,
                    limit: null,
                    cancellationToken)),
            ReportKind.DailySite => BuildDailySite(
                await reporting.GetDailySiteAsync(
                    filter,
                    limit: null,
                    cancellationToken)),
            ReportKind.Procurement => BuildProcurement(
                await reporting.GetProcurementAsync(
                    filter,
                    limit: null,
                    cancellationToken)),
            ReportKind.Commercial => BuildCommercial(
                await reporting.GetCommercialAsync(
                    filter,
                    limit: null,
                    cancellationToken)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(report),
                report,
                "Unsupported report type.")
        };
    }

    private static ReportCsvDocument BuildPortfolio(
        ReportResult<PortfolioReportRow> report)
    {
        var rows = new List<IReadOnlyList<string?>>
        {
            new[]
            {
                "Project Number",
                "Project Name",
                "Client",
                "Location",
                "Status",
                "Start Date",
                "Target Completion",
                "Latest Daily Report",
                "Open Site Issues",
                "Pending Approvals"
            }
        };

        rows.AddRange(report.Items.Select(item =>
            (IReadOnlyList<string?>)
            [
                item.ProjectNumber,
                item.ProjectName,
                item.ClientName,
                item.Location,
                item.Status.ToString(),
                FormatDate(item.StartDate),
                FormatDate(item.TargetCompletionDate),
                FormatDate(item.LatestDailyReportDate),
                item.OpenSiteIssues.ToString(CultureInfo.InvariantCulture),
                item.PendingApprovals.ToString(CultureInfo.InvariantCulture)
            ]));

        return Build("project-portfolio.csv", rows);
    }

    private static ReportCsvDocument BuildDailySite(
        ReportResult<DailySiteReportRow> report)
    {
        var rows = new List<IReadOnlyList<string?>>
        {
            new[]
            {
                "Project Number",
                "Project Name",
                "Report Date",
                "Status",
                "Prepared By",
                "Weather",
                "Activities",
                "Manpower Headcount",
                "Equipment Quantity",
                "Open Site Issues"
            }
        };

        rows.AddRange(report.Items.Select(item =>
            (IReadOnlyList<string?>)
            [
                item.ProjectNumber,
                item.ProjectName,
                FormatDate(item.ReportDate),
                item.Status.ToString(),
                item.PreparedBy,
                item.Weather,
                item.ActivityCount.ToString(CultureInfo.InvariantCulture),
                item.ManpowerHeadcount.ToString(CultureInfo.InvariantCulture),
                item.EquipmentQuantity.ToString(CultureInfo.InvariantCulture),
                item.OpenSiteIssues.ToString(CultureInfo.InvariantCulture)
            ]));

        return Build("daily-site-register.csv", rows);
    }

    private static ReportCsvDocument BuildProcurement(
        ReportResult<ProcurementReportRow> report)
    {
        var rows = new List<IReadOnlyList<string?>>
        {
            new[]
            {
                "Project Number",
                "Project Name",
                "Purchase Request",
                "Purchase Order",
                "Supplier",
                "Status",
                "Order Date",
                "Expected Delivery",
                "Currency",
                "PO Total",
                "Delivery Count",
                "Latest Delivery"
            }
        };

        rows.AddRange(report.Items.Select(item =>
            (IReadOnlyList<string?>)
            [
                item.ProjectNumber,
                item.ProjectName,
                item.PurchaseRequestNumber,
                item.PurchaseOrderNumber,
                item.SupplierName,
                item.Status.ToString(),
                FormatDate(item.OrderDate),
                FormatDate(item.ExpectedDeliveryDate),
                item.Currency,
                FormatMoney(item.PurchaseOrderTotal),
                item.DeliveryCount.ToString(CultureInfo.InvariantCulture),
                FormatDate(item.LatestDeliveryDate)
            ]));

        return Build("procurement-register.csv", rows);
    }

    private static ReportCsvDocument BuildCommercial(
        ReportResult<CommercialReportRow> report)
    {
        var rows = new List<IReadOnlyList<string?>>
        {
            new[]
            {
                "Project Number",
                "Project Name",
                "Purchase Order",
                "Supplier",
                "Payment Application",
                "Application Date",
                "Application Status",
                "Currency",
                "Claimed Amount",
                "Certificate Number",
                "Certificate Date",
                "Certification Status",
                "Certified Amount",
                "Payable Amount"
            }
        };

        rows.AddRange(report.Items.Select(item =>
            (IReadOnlyList<string?>)
            [
                item.ProjectNumber,
                item.ProjectName,
                item.PurchaseOrderNumber,
                item.SupplierName,
                item.ApplicationNumber,
                FormatDate(item.ApplicationDate),
                FormatStatus(item.ApplicationStatus),
                item.Currency,
                FormatMoney(item.ClaimedAmount),
                item.CertificateNumber,
                FormatDate(item.CertificateDate),
                item.CertificationStatus is null
                    ? null
                    : FormatStatus(item.CertificationStatus.Value),
                FormatMoney(item.CertifiedAmount),
                FormatMoney(item.PayableAmount)
            ]));

        return Build("commercial-register.csv", rows);
    }

    private static ReportCsvDocument Build(
        string fileName,
        IEnumerable<IReadOnlyList<string?>> rows)
    {
        var builder = new StringBuilder();

        foreach (var row in rows)
        {
            builder.AppendLine(
                string.Join(",", row.Select(Escape)));
        }

        var body = Utf8WithBom.GetBytes(builder.ToString());
        var preamble = Utf8WithBom.GetPreamble();
        var content = new byte[preamble.Length + body.Length];

        preamble.CopyTo(content, 0);
        body.CopyTo(content, preamble.Length);

        return new ReportCsvDocument(
            fileName,
            content);
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;

        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        return value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{value}\""
            : value;
    }

    private static string FormatDate(DateOnly? value) =>
        value?.ToString(
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture)
        ?? string.Empty;

    private static string FormatMoney(decimal? value) =>
        value?.ToString(
            "0.00",
            CultureInfo.InvariantCulture)
        ?? string.Empty;

    private static string FormatStatus<TStatus>(TStatus status)
        where TStatus : struct, Enum
    {
        var raw = status.ToString();
        var builder = new StringBuilder();

        for (var index = 0; index < raw.Length; index++)
        {
            if (index > 0 &&
                char.IsUpper(raw[index]) &&
                !char.IsUpper(raw[index - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(raw[index]);
        }

        return builder.ToString();
    }
}
