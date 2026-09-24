using Application.Approvals;
using Application.Common.Authentication;
using Application.Projects;
using Application.Users;
using Domain.Approvals;
using Domain.Projects;
using Xunit;

namespace Application.Tests;

public sealed class ApprovalRequestHandlerTests
{
    [Fact]
    public async Task Create_rejects_inactive_approver()
    {
        var repository = new FakeApprovalRepository();
        var approverId = Guid.NewGuid();

        var handler = new CreateApprovalRequestHandler(
            repository,
            new FakeProjectRepository(),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "approver@example.com",
                    false)),
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateApprovalRequestCommand(
                null,
                "PurchaseRequest",
                Guid.NewGuid(),
                "PR-001",
                "Purchase request approval",
                null,
                [
                    new ApprovalStepInput(
                        "Project Manager",
                        approverId)
                ]));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Create_rejects_duplicate_pending_subject()
    {
        var repository = new FakeApprovalRepository
        {
            HasPending = true
        };

        var approverId = Guid.NewGuid();

        var handler = new CreateApprovalRequestHandler(
            repository,
            new FakeProjectRepository(),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "approver@example.com",
                    true)),
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreateApprovalRequestCommand(
                null,
                "PurchaseRequest",
                Guid.NewGuid(),
                "PR-001",
                "Purchase request approval",
                null,
                [
                    new ApprovalStepInput(
                        "Project Manager",
                        approverId)
                ]));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    private sealed class FakeApprovalRepository
        : IApprovalRequestRepository
    {
        public bool HasPending { get; set; }

        public List<ApprovalRequest> Added { get; } = [];

        public Task<ApprovalRequest?> GetByIdAsync(
            Guid approvalRequestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ApprovalRequest?>(null);

        public Task<bool> HasPendingForSubjectAsync(
            string subjectType,
            Guid subjectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HasPending);

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
                _entries.Where(user => userIds.Contains(user.Id)).ToArray());
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

    private sealed class FakeProjectRepository
        : IProjectRepository
    {
        public Task<Project?> GetByIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Project?>(null);

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
}
