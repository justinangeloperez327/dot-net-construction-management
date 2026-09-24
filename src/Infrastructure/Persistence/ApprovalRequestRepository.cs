using Application.Approvals;
using Domain.Approvals;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class ApprovalRequestRepository(
    ApplicationDbContext dbContext)
    : IApprovalRequestRepository
{
    public Task<ApprovalRequest?> GetByIdAsync(
        Guid approvalRequestId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ApprovalRequests
            .Include(request => request.Steps)
            .SingleOrDefaultAsync(
                request => request.Id == approvalRequestId,
                cancellationToken);
    }

    public Task<bool> HasPendingForSubjectAsync(
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ApprovalRequests
            .AnyAsync(
                request =>
                    request.SubjectType == subjectType &&
                    request.SubjectId == subjectId &&
                    request.Status == ApprovalRequestStatus.Pending,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalRequest>> ListForUserAsync(
        Guid userId,
        ApprovalRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ApprovalRequests
            .AsNoTracking()
            .Include(request => request.Steps)
            .Where(request =>
                request.RequestedByUserId == userId ||
                request.Steps.Any(
                    step => step.ApproverUserId == userId));

        if (status is not null)
        {
            query = query.Where(
                request => request.Status == status.Value);
        }

        return await query
            .OrderByDescending(request => request.RequestedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountForUserAsync(
        Guid userId,
        ApprovalRequestStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.ApprovalRequests
            .AsNoTracking()
            .Where(request =>
                request.RequestedByUserId == userId ||
                request.Steps.Any(
                    step => step.ApproverUserId == userId));

        if (status is not null)
        {
            query = query.Where(
                request => request.Status == status.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        await dbContext.ApprovalRequests.AddAsync(
            request,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
