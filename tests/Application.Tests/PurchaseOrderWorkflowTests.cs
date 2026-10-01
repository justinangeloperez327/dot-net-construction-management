using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.Projects;
using Application.PurchaseOrders;
using Application.PurchaseRequests;
using Application.Suppliers;
using Application.Users;
using Domain.Approvals;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.PurchaseRequests;
using Domain.Suppliers;
using Xunit;

namespace Application.Tests;

public sealed class PurchaseOrderWorkflowTests
{
    [Fact]
    public async Task Create_copies_approved_purchase_request_items()
    {
        var creatorId = Guid.NewGuid();
        var purchaseRequest = CreateApprovedPurchaseRequest();
        var supplier = CreateSupplier();
        var repository = new FakePurchaseOrderRepository();

        var handler = new CreatePurchaseOrderHandler(
            repository,
            new FakePurchaseRequestRepository(purchaseRequest),
            new FakeSupplierRepository(supplier),
            new FakeProjectRepository(CreateProject()),
            new FakeCurrentUser(creatorId),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreatePurchaseOrderCommand(
                purchaseRequest.Id,
                supplier.Id,
                "PO-001",
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 15),
                "aed",
                "Project site",
                null,
                "30 days",
                null));

        Assert.True(result.Succeeded);
        Assert.Single(repository.Added);

        var order = repository.Added.Single();

        Assert.Equal("PO-001", order.PurchaseOrderNumber);
        Assert.Equal("AED", order.Currency);
        Assert.Equal(supplier.Id, order.SupplierId);
        Assert.Equal(creatorId, order.CreatedByUserId);
        Assert.Equal(purchaseRequest.Items.Count, order.Items.Count);

        Assert.All(
            order.Items,
            item => Assert.Equal(0m, item.UnitPrice));
    }

    [Fact]
    public async Task Submit_rejects_split_award_overallocation()
    {
        var creatorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var purchaseRequest = CreateApprovedPurchaseRequest();
        var sourceItem = purchaseRequest.Items.First();
        var supplier = CreateSupplier();
        var order = CreateOrderFromRequest(
            purchaseRequest,
            supplier,
            creatorId);

        order.UpdateItem(
            order.Items.First().Id,
            sourceItem.Quantity,
            100m,
            0m,
            5m,
            null);

        var purchaseOrders = new FakePurchaseOrderRepository(order)
        {
            CommittedQuantity = 1m
        };

        var approvals = new FakeApprovalRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(
            purchaseOrders,
            new FakePurchaseRequestRepository(purchaseRequest),
            approvals,
            new FakeSupplierRepository(supplier),
            new FakeProjectRepository(CreateProject()),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "approver@example.com",
                    true)),
            new FakeCurrentUser(creatorId),
            unitOfWork,
            TimeProvider.System);

        var result = await handler.HandleAsync(
            order.Id,
            new SubmitPurchaseOrderCommand(
                [
                    new ApprovalStepInput(
                        "Commercial Manager",
                        approverId)
                ]));

        Assert.False(result.Succeeded);
        Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
        Assert.Empty(approvals.Added);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Submit_creates_approval_and_commits_once()
    {
        var creatorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var purchaseRequest = CreateApprovedPurchaseRequest();
        var supplier = CreateSupplier();
        var order = CreateOrderFromRequest(
            purchaseRequest,
            supplier,
            creatorId);

        foreach (var item in order.Items)
        {
            order.UpdateItem(
                item.Id,
                item.Quantity,
                25m,
                0m,
                5m,
                null);
        }

        var approvals = new FakeApprovalRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(
            new FakePurchaseOrderRepository(order),
            new FakePurchaseRequestRepository(purchaseRequest),
            approvals,
            new FakeSupplierRepository(supplier),
            new FakeProjectRepository(CreateProject()),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "approver@example.com",
                    true)),
            new FakeCurrentUser(creatorId),
            unitOfWork,
            TimeProvider.System);

        var result = await handler.HandleAsync(
            order.Id,
            new SubmitPurchaseOrderCommand(
                [
                    new ApprovalStepInput(
                        "Commercial Manager",
                        approverId)
                ]));

        Assert.True(result.Succeeded);
        Assert.Equal(
            PurchaseOrderStatus.PendingApproval,
            order.Status);
        Assert.Single(approvals.Added);
        Assert.Equal(
            PurchaseOrderApproval.SubjectType,
            approvals.Added.Single().SubjectType);
        Assert.Equal(order.Id, approvals.Added.Single().SubjectId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Approval_outcome_updates_purchase_order()
    {
        var creatorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var purchaseRequest = CreateApprovedPurchaseRequest();
        var supplier = CreateSupplier();
        var order = CreateOrderFromRequest(
            purchaseRequest,
            supplier,
            creatorId);

        foreach (var item in order.Items)
        {
            order.UpdateItem(
                item.Id,
                item.Quantity,
                25m,
                0m,
                0m,
                null);
        }

        var approval = ApprovalRequest.Create(
            order.ProjectId,
            PurchaseOrderApproval.SubjectType,
            order.Id,
            order.PurchaseOrderNumber,
            "Purchase order approval",
            null,
            creatorId,
            DateTimeOffset.UtcNow,
            [
                new ApprovalStepAssignment(
                    "Commercial Manager",
                    approverId)
            ]);

        order.SubmitForApproval(approval.Id);

        approval.Decide(
            approval.CurrentStep!.Id,
            approverId,
            ApprovalStepDecision.Approve,
            DateTimeOffset.UtcNow,
            null);

        var handler = new PurchaseOrderApprovalOutcomeHandler(
            new FakePurchaseOrderRepository(order));

        await handler.ApplyAsync(approval);

        Assert.Equal(PurchaseOrderStatus.Approved, order.Status);
    }

    private static PurchaseRequest CreateApprovedPurchaseRequest()
    {
        var request = PurchaseRequest.Create(
            Guid.NewGuid(),
            "PR-001",
            "Site materials",
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        request.AddItem(
            "Cement board",
            10m,
            "pcs",
            null);

        request.AddItem(
            "Fasteners",
            100m,
            "pcs",
            null);

        var approvalId = Guid.NewGuid();
        request.SubmitForApproval(approvalId);
        request.ApplyApprovalOutcome(
            approvalId,
            PurchaseRequestStatus.Approved);

        return request;
    }

    private static PurchaseOrder CreateOrderFromRequest(
        PurchaseRequest purchaseRequest,
        Supplier supplier,
        Guid creatorId)
    {
        var order = PurchaseOrder.Create(
            purchaseRequest.ProjectId,
            purchaseRequest.Id,
            supplier.Id,
            "PO-001",
            new DateOnly(2026, 10, 1),
            null,
            "AED",
            null,
            null,
            null,
            null,
            creatorId,
            DateTimeOffset.UtcNow);

        foreach (var sourceItem in purchaseRequest.Items)
        {
            order.AddItem(
                sourceItem.Id,
                sourceItem.Description,
                sourceItem.Quantity,
                sourceItem.Unit,
                0m,
                0m,
                0m,
                sourceItem.Remarks);
        }

        return order;
    }

    private static Supplier CreateSupplier() =>
        Supplier.Create(
            "SUP-001",
            "Supplier",
            "General Trading",
            null,
            null,
            null,
            null,
            null,
            null);

    private static Project CreateProject() =>
        Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 1),
            null,
            null);

    private sealed class FakePurchaseOrderRepository
        : IPurchaseOrderRepository
    {
        private readonly PurchaseOrder? _order;

        public FakePurchaseOrderRepository()
        {
        }

        public FakePurchaseOrderRepository(PurchaseOrder order)
        {
            _order = order;
        }

        public decimal CommittedQuantity { get; set; }

        public List<PurchaseOrder> Added { get; } = [];

        public Task<PurchaseOrder?> GetByIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _order?.Id == purchaseOrderId
                    ? _order
                    : null);

        public Task<bool> PurchaseOrderNumberExistsAsync(
            string purchaseOrderNumber,
            Guid? excludingPurchaseOrderId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<decimal> GetCommittedQuantityForSourceItemAsync(
            Guid purchaseRequestItemId,
            Guid excludingPurchaseOrderId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CommittedQuantity);

        public Task<IReadOnlyList<PurchaseOrder>> ListForProjectAsync(
            Guid projectId,
            PurchaseOrderStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PurchaseOrder>>([]);

        public Task<IReadOnlyList<PurchaseOrder>> ListByIdsAsync(
            IReadOnlyCollection<Guid> purchaseOrderIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PurchaseOrder>>(
                _order is not null &&
                purchaseOrderIds.Contains(_order.Id)
                    ? [_order]
                    : []);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            PurchaseOrderStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            PurchaseOrder purchaseOrder,
            CancellationToken cancellationToken = default)
        {
            Added.Add(purchaseOrder);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

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

    private sealed class FakeSupplierRepository(Supplier supplier)
        : ISupplierRepository
    {
        public Task<Supplier?> GetByIdAsync(
            Guid supplierId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Supplier?>(
                supplier.Id == supplierId ? supplier : null);

        public Task<bool> SupplierCodeExistsAsync(
            string supplierCode,
            Guid? excludingSupplierId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<IReadOnlyList<Supplier>> ListAsync(
            string? search,
            bool? isActive,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Supplier>>([]);

        public Task<IReadOnlyList<Supplier>> ListActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Supplier>>([supplier]);

        public Task<IReadOnlyList<Supplier>> ListByIdsAsync(
            IReadOnlyCollection<Guid> supplierIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Supplier>>(
                supplierIds.Contains(supplier.Id)
                    ? [supplier]
                    : []);

        public Task<int> CountAsync(
            string? search,
            bool? isActive,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Supplier supplier,
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
                    "buyer@example.com",
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
