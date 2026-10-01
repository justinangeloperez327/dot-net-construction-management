using Application.Security;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Security;

public sealed class SecurityEventStore(
    ApplicationDbContext dbContext)
    : ISecurityEventRecorder,
      ISecurityEventQueryService
{
    public async Task RecordAsync(
        SecurityEventRecord securityEvent,
        CancellationToken cancellationToken = default)
    {
        var log = new SecurityEventLog(
            securityEvent.EventType.ToString(),
            securityEvent.UserId,
            Truncate(securityEvent.Email, 320),
            Truncate(securityEvent.RemoteIpAddress, 64),
            Truncate(securityEvent.UserAgent, 512),
            Truncate(securityEvent.RequestPath, 2048),
            securityEvent.OccurredAt);

        await dbContext.SecurityEvents.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SecurityEventResult> ListAsync(
        SecurityEventQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<SecurityEventLog> source =
            dbContext.SecurityEvents.AsNoTracking();

        if (query.EventType is SecurityEventType eventType)
        {
            var name = eventType.ToString();
            source = source.Where(item => item.EventType == name);
        }

        if (!string.IsNullOrWhiteSpace(query.Email))
        {
            var email = query.Email.Trim();
            source = source.Where(item =>
                item.Email != null && item.Email.Contains(email));
        }

        if (query.FromDate is DateOnly fromDate)
        {
            var from = new DateTimeOffset(
                fromDate.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);
            source = source.Where(item => item.OccurredAt >= from);
        }

        if (query.ToDate is DateOnly toDate)
        {
            var toExclusive = new DateTimeOffset(
                toDate.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero).AddDays(1);
            source = source.Where(item => item.OccurredAt < toExclusive);
        }

        var totalCount = await source.CountAsync(cancellationToken);

        var rows = await source
            .OrderByDescending(item => item.OccurredAt)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(item => new SecurityEventItem(
            item.Id,
            Enum.TryParse<SecurityEventType>(
                item.EventType,
                ignoreCase: true,
                out var parsed)
                ? parsed
                : SecurityEventType.LoginFailed,
            item.UserId,
            item.Email,
            item.RemoteIpAddress,
            item.UserAgent,
            item.RequestPath,
            item.OccurredAt))
            .ToArray();

        return new SecurityEventResult(
            items,
            page,
            pageSize,
            totalCount);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();

        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
