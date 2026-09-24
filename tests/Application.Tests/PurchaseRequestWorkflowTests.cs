using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.Projects;
using Application.PurchaseRequests;
using Application.Users;
using Domain.Approvals;
using Domain.Projects;
using Domain.PurchaseRequests;
using Xunit;

namespace Application.Tests;

public sealed class PurchaseRequestWorkflowTests
{
    [Fact]
    public async Task Submit_creates_approval_and_moves_request_to_pending()
    {
        var requesterId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var purchaseRequest = CreateRequest(requesterId);
        purchaseRequest.AddItem(
            "Cement board",
            10,
            "pcs",
            null);

        var purchaseRequests =
            new FakePurchaseRequestRepository(purchaseRequest);
        var approvals = new FakeApprovalRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseRequestHandler(
            purchaseRequests,
            approvals,
            new FakeProjectRepository(CreateProject()),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "approver@example.com",
                    true)),
            new FakeCurrentUser(requesterId),
            unitOfWork,
            TimeProvider.System);

        var result = await handler.HandleAsync(
            purchaseRequest.Id,
            new SubmitPurchaseRequestCommand(
                [
                    new ApprovalStepInput(
                        "Project Manager",
                        approverId)
                ]));

        Assert.True(result.Succeeded);
        Assert.Equal(
            PurchaseRequestStatus.PendingApproval,
            purchaseRequest.Status);
        Assert.NotNull(purchaseRequest.ApprovalRequestId);
        Assert.Single(approvals.Added);
        Assert.Equal(
            PurchaseRequestApproval.SubjectType,
            approvals.Added.Single().SubjectType);
        Assert.Equal(
            purchaseRequest.Id,
            approvals.Added.Single().SubjectId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Submit_rejects_inactive_approver()
    {
        var requesterId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var purchaseRequest = CreateRequest(requesterId);
        purchaseRequest.AddItem(
            "Cement board",
            10,
            "pcs",
            null);

        var approvals = new FakeApprovalRepository();

        var handler = new SubmitPurchaseRequestHandler(
            new FakePurchaseRequestRepository(purchaseRequest),
            approvals,
            new FakeProjectRepository(CreateProject()),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "inactive@example.com",
                    false)),
            new FakeCurrentUser(requesterId),
            new FakeUnitOfWork(),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            purchaseRequest.Id,
            new SubmitPurchaseRequestCommand(
                [
                    new ApprovalStepInput(
                        "Project Manager",
                        approverId)
                ]));

        Assert.False(result.Succeeded);
        Assert.Equal(
            PurchaseRequestStatus.Draft,
            purchaseRequest.Status);
        Assert.Empty(approvals.Added);
    }

    [Fact]
    public async Task Approval_outcome_handler_updates_purchase_request()
    {
        var requesterId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var purchaseRequest = CreateRequest(requesterId);
        purchaseRequest.AddItem(
            "Cement board",
            10,
            "pcs",
            null);

        var approval = ApprovalRequest.Create(
            purchaseRequest.ProjectId,
            PurchaseRequestApproval.SubjectType,
            purchaseRequest.Id,
            purchaseRequest.RequestNumber,
            "Purchase request approval",
            null,
            requesterId,
            DateTimeOffset.UtcNow,
            [
                new ApprovalStepAssignment(
                    "Project Manager",
                    approverId)
            ]);

        purchaseRequest.SubmitForApproval(approval.Id);

        approval.Decide(
            approval.CurrentStep!.Id,
            approverId,
            ApprovalStepDecision.Approve,
            DateTimeOffset.UtcNow,
            null);

        var handler = new PurchaseRequestApprovalOutcomeHandler(
            new FakePurchaseRequestRepository(purchaseRequest));

        await handler.ApplyAsync(approval);

        Assert.Equal(
            PurchaseRequestStatus.Approved,
            purchaseRequest.Status);
    }

    private static PurchaseRequest CreateRequest(Guid requesterId) =>
        PurchaseRequest.Create(
            Guid.NewGuid(),
            "PR-001",
            "Site materials",
            null,
            null,
            requesterId,
            DateTimeOffset.UtcNow);

    private static Project CreateProject() =>
        Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 1),
            null,
            null);

    private sealed class FakePurchaseRequestRepository(
        PurchaseRequest purchaseRequest)
        : IPurchaseRequestRepository
    {
        public Task<PurchaseRequest?> GetByIdAsync(
            Guid purchaseRequestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PurchaseRequest?>(
                purchaseRequest.Id == purchaseRequestId
                    ? purchaseRequest
                    : null);

        public Task<bool> RequestNumberExistsAsync(
            Guid projectId,
            string requestNumber,
            Guid? excludingPurchaseRequestId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<PurchaseRequest>> ListForProjectAsync(
            Guid projectId,
            PurchaseRequestStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PurchaseRequest>>([]);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            PurchaseRequestStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            PurchaseRequest purchaseRequest,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeApprovalRepository
        : IApprovalRequestRepository
    {
        public List<ApprovalRequest> Added { get; } = [];

        public Task<ApprovalRequest?> GetByIdAsync(
            Guid approvalRequestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ApprovalRequest?>(null);

        public Task<bool> HasPendingForSubjectAsync(
            string subjectType,
            Guid subjectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<ApprovalRequest>> ListForUserAsync(
            Guid userId,
            ApprovalRequestStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ApprovalRequest>>([]);

        public Task<int> CountForUserAsync(
            Guid userId,
            ApprovalRequestStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            ApprovalRequest request,
            CancellationToken cancellationToken = default)
        {
            Added.Add(request);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeProjectRepository(Project project)
        : IProjectRepository
    {
        public Task<Project?> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Project?>(project);

        public Task<bool> ProjectNumberExistsAsync(
            string projectNumber,
            Guid? excludingProjectId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<Project>> ListAsync(
            string? search,
            ProjectStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Project>>([]);

        public Task<int> CountAsync(
            string? search,
            ProjectStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeUserDirectory(
        params UserDirectoryEntry[] entries)
        : IUserDirectory
    {
        private readonly IReadOnlyList<UserDirectoryEntry> _entries =
            entries;

        public Task<UserDirectoryEntry?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _entries.SingleOrDefault(user => user.Id == userId));

        public Task<IReadOnlyList<UserDirectoryEntry>> ListActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<UserDirectoryEntry>>(
                _entries.Where(user => user.IsActive).ToArray());

        public Task<IReadOnlyList<UserDirectoryEntry>> ListByIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<UserDirectoryEntry>>(
                _entries.Where(user => userIds.Contains(user.Id))
                    .ToArray());
    }

    private sealed class FakeCurrentUser(Guid userId)
        : ICurrentUser
    {
        public ValueTask<CurrentUserInfo> GetAsync(
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(
                new CurrentUserInfo(
                    userId,
                    "requester@example.com",
                    true));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
