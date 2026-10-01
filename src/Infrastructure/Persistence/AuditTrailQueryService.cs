using Application.Auditing;
using Infrastructure.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AuditTrailQueryService(
    ApplicationDbContext dbContext)
    : IAuditTrailQueryService
{
    public async Task<AuditTrailResult> ListAsync(
        AuditTrailQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<AuditLog> auditQuery =
            dbContext.AuditLogs.AsNoTracking();

        if (query.ProjectId is Guid projectId)
        {
            auditQuery = auditQuery.Where(
                audit => audit.ProjectId == projectId);
        }

        if (query.Action is AuditAction action)
        {
            var actionName = action.ToString();

            auditQuery = auditQuery.Where(
                audit => audit.Action == actionName);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var entityType = query.EntityType.Trim();

            auditQuery = auditQuery.Where(
                audit => audit.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(query.Actor))
        {
            var actor = query.Actor.Trim();

            auditQuery = auditQuery.Where(
                audit =>
                    audit.ActorEmail != null &&
                    audit.ActorEmail.Contains(actor));
        }

        if (query.FromDate is DateOnly fromDate)
        {
            var from = new DateTimeOffset(
                fromDate.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);

            auditQuery = auditQuery.Where(
                audit => audit.OccurredAt >= from);
        }

        if (query.ToDate is DateOnly toDate)
        {
            var toExclusive = new DateTimeOffset(
                    toDate.ToDateTime(TimeOnly.MinValue),
                    TimeSpan.Zero)
                .AddDays(1);

            auditQuery = auditQuery.Where(
                audit => audit.OccurredAt < toExclusive);
        }

        var totalCount = await auditQuery.CountAsync(
            cancellationToken);

        var rows = await auditQuery
            .OrderByDescending(audit => audit.OccurredAt)
            .ThenByDescending(audit => audit.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(audit => new
            {
                audit.Id,
                audit.OccurredAt,
                audit.ProjectId,
                audit.ActorUserId,
                audit.ActorEmail,
                audit.Action,
                audit.EntityType,
                audit.EntityId,
                audit.ChangesJson
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new AuditTrailItem(
                row.Id,
                row.OccurredAt,
                row.ProjectId,
                row.ActorUserId,
                row.ActorEmail
                    ?? (row.ActorUserId is null
                        ? "System"
                        : row.ActorUserId.Value.ToString()),
                Enum.TryParse<AuditAction>(
                    row.Action,
                    ignoreCase: true,
                    out var parsedAction)
                    ? parsedAction
                    : AuditAction.Updated,
                row.EntityType,
                row.EntityId,
                row.ChangesJson))
            .ToArray();

        return new AuditTrailResult(
            items,
            page,
            pageSize,
            totalCount);
    }

    public async Task<IReadOnlyList<string>> ListEntityTypesAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .AsNoTracking()
            .Select(audit => audit.EntityType)
            .Distinct()
            .OrderBy(entityType => entityType)
            .ToListAsync(cancellationToken);
    }
}
