using Application.Approvals;
using Application.Common.Authentication;
using Application.Common.Persistence;
using Application.Deliveries;
using Application.PaymentApplications;
using Application.Projects;
using Application.PurchaseOrders;
using Application.Suppliers;
using Application.Users;
using Domain.Approvals;
using Domain.Deliveries;
using Domain.PaymentApplications;
using Domain.Projects;
using Domain.PurchaseOrders;
using Domain.Suppliers;
using Xunit;

namespace Application.Tests;

public sealed class PaymentApplicationWorkflowTests
{
    [Fact]
    public async Task Create_requires_unclaimed_received_quantity()
    {
        var order = CreateApprovedOrder();
        var repository = new FakePaymentApplicationRepository
        {
            CommittedClaimQuantity = 10m
        };

        var handler = new CreatePaymentApplicationHandler(
            repository,
            new FakePurchaseOrderRepository(order),
            new FakeDeliveryRepository
            {
                ReceivedQuantity = 10m
            },
            new FakeProjectRepository(CreateProject()),
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new CreatePaymentApplicationCommand(
                order.Id,
                "PA-001",
                new DateOnly(2026, 10, 2),
                null,
                null,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Add_item_rejects_claim_above_available_received_quantity()
    {
        var order = CreateApprovedOrder();
        var application = CreateApplication(order);

        var repository = new FakePaymentApplicationRepository(application)
        {
            CommittedClaimQuantity = 4m
        };

        var handler = new AddPaymentApplicationItemHandler(
            repository,
            new FakePurchaseOrderRepository(order),
            new FakeDeliveryRepository
            {
                ReceivedQuantity = 5m
            });

        var result = await handler.HandleAsync(
            application.Id,
            new PaymentApplicationItemCommand(
                order.Items.Single().Id,
                2m,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(application.Items);
    }

    [Fact]
    public async Task Submit_creates_approval_and_commits_once()
    {
        var creatorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var order = CreateApprovedOrder();
        var application = PaymentApplication.Create(
            order.ProjectId,
            order.Id,
            "PA-001",
            new DateOnly(2026, 10, 2),
            null,
            null,
            null,
            creatorId,
            DateTimeOffset.UtcNow);

        var orderItem = order.Items.Single();

        application.AddItem(
            orderItem.Id,
            orderItem.Description,
            5m,
            orderItem.Unit,
            orderItem.UnitPrice,
            orderItem.DiscountPercent,
            orderItem.TaxPercent,
            null);

        var approvals = new FakeApprovalRepository();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPaymentApplicationHandler(
            new FakePaymentApplicationRepository(application),
            new FakePurchaseOrderRepository(order),
            new FakeDeliveryRepository
            {
                ReceivedQuantity = 10m
            },
            approvals,
            new FakeSupplierRepository(CreateSupplier()),
            new FakeUserDirectory(
                new UserDirectoryEntry(
                    approverId,
                    "commercial@example.com",
                    true)),
            new FakeCurrentUser(creatorId),
            unitOfWork,
            TimeProvider.System);

        var result = await handler.HandleAsync(
            application.Id,
            new SubmitPaymentApplicationCommand(
                [
                    new ApprovalStepInput(
                        "Commercial Manager",
                        approverId)
                ]));

        Assert.True(result.Succeeded);
        Assert.Equal(
            PaymentApplicationStatus.PendingApproval,
            application.Status);
        Assert.Single(approvals.Added);
        Assert.Equal(
            PaymentApplicationApproval.SubjectType,
            approvals.Added.Single().SubjectType);
        Assert.Equal(application.Id, approvals.Added.Single().SubjectId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Approval_outcome_updates_payment_application()
    {
        var creatorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var order = CreateApprovedOrder();
        var application = PaymentApplication.Create(
            order.ProjectId,
            order.Id,
            "PA-001",
            new DateOnly(2026, 10, 2),
            null,
            null,
            null,
            creatorId,
            DateTimeOffset.UtcNow);

        var orderItem = order.Items.Single();

        application.AddItem(
            orderItem.Id,
            orderItem.Description,
            5m,
            orderItem.Unit,
            orderItem.UnitPrice,
            orderItem.DiscountPercent,
            orderItem.TaxPercent,
            null);

        var approval = ApprovalRequest.Create(
            application.ProjectId,
            PaymentApplicationApproval.SubjectType,
            application.Id,
            application.ApplicationNumber,
            "Payment application approval",
            null,
            creatorId,
            DateTimeOffset.UtcNow,
            [
                new ApprovalStepAssignment(
                    "Commercial Manager",
                    approverId)
            ]);

        application.SubmitForApproval(approval.Id);

        approval.Decide(
            approval.CurrentStep!.Id,
            approverId,
            ApprovalStepDecision.Approve,
            DateTimeOffset.UtcNow,
            null);

        var handler = new PaymentApplicationApprovalOutcomeHandler(
            new FakePaymentApplicationRepository(application));

        await handler.ApplyAsync(approval);

        Assert.Equal(
            PaymentApplicationStatus.Approved,
            application.Status);
    }

    private static PaymentApplication CreateApplication(
        PurchaseOrder order) =>
        PaymentApplication.Create(
            order.ProjectId,
            order.Id,
            "PA-001",
            new DateOnly(2026, 10, 2),
            null,
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

    private static PurchaseOrder CreateApprovedOrder()
    {
        var order = PurchaseOrder.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "PO-001",
            new DateOnly(2026, 10, 1),
            null,
            "AED",
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        order.AddItem(
            Guid.NewGuid(),
            "Cement board",
            10m,
            "pcs",
            20m,
            0m,
            5m,
            null);

        var approvalId = Guid.NewGuid();
        order.SubmitForApproval(approvalId);
        order.ApplyApprovalOutcome(
            approvalId,
            PurchaseOrderStatus.Approved);

        return order;
    }

    private static Project CreateProject() =>
        Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 1),
            null,
            null);

    private static Supplier CreateSupplier() =>
        Supplier.Create(
            "SUP-001",
            "Supplier",
            null,
            null,
            null,
            null,
            null,
            null,
            null);

    private sealed class FakePaymentApplicationRepository
        : IPaymentApplicationRepository
    {
        private readonly PaymentApplication? _application;

        public FakePaymentApplicationRepository()
        {
        }

        public FakePaymentApplicationRepository(
            PaymentApplication application)
        {
            _application = application;
        }

        public decimal CommittedClaimQuantity { get; set; }

        public List<PaymentApplication> Added { get; } = [];

        public Task<PaymentApplication?> GetByIdAsync(
            Guid paymentApplicationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _application?.Id == paymentApplicationId
                    ? _application
                    : null);

        public Task<bool> ApplicationNumberExistsAsync(
            Guid purchaseOrderId,
            string applicationNumber,
            Guid? excludingPaymentApplicationId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<decimal> GetCommittedClaimQuantityForPurchaseOrderItemAsync(
            Guid purchaseOrderItemId,
            Guid excludingPaymentApplicationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CommittedClaimQuantity);

        public Task<IReadOnlyList<PaymentApplication>> ListForProjectAsync(
            Guid projectId,
            PaymentApplicationStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PaymentApplication>>([]);

        public Task<IReadOnlyList<PaymentApplication>> ListForPurchaseOrderAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PaymentApplication>>([]);

        public Task<IReadOnlyList<PaymentApplication>> ListByIdsAsync(
            IReadOnlyCollection<Guid> paymentApplicationIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PaymentApplication>>(
                _application is not null &&
                paymentApplicationIds.Contains(_application.Id)
                    ? [_application]
                    : []);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            PaymentApplicationStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            PaymentApplication paymentApplication,
            CancellationToken cancellationToken = default)
        {
            Added.Add(paymentApplication);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakePurchaseOrderRepository(PurchaseOrder order)
        : IPurchaseOrderRepository
    {
        public Task<PurchaseOrder?> GetByIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PurchaseOrder?>(
                order.Id == purchaseOrderId ? order : null);

        public Task<bool> PurchaseOrderNumberExistsAsync(
            string purchaseOrderNumber,
            Guid? excludingPurchaseOrderId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<decimal> GetCommittedQuantityForSourceItemAsync(
            Guid purchaseRequestItemId,
            Guid excludingPurchaseOrderId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0m);

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
                purchaseOrderIds.Contains(order.Id)
                    ? [order]
                    : []);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            PurchaseOrderStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            PurchaseOrder purchaseOrder,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeDeliveryRepository
        : IDeliveryRepository
    {
        public decimal ReceivedQuantity { get; set; }

        public Task<Delivery?> GetByIdAsync(
            Guid deliveryId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Delivery?>(null);

        public Task<bool> DeliveryNoteExistsAsync(
            Guid purchaseOrderId,
            string deliveryNoteNumber,
            Guid? excludingDeliveryId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<decimal> GetReceivedQuantityForPurchaseOrderItemAsync(
            Guid purchaseOrderItemId,
            Guid? excludingDeliveryId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ReceivedQuantity);

        public Task<IReadOnlyList<Delivery>> ListForProjectAsync(
            Guid projectId,
            DeliveryStatus? status,
            int skip,
            int take,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Delivery>>([]);

        public Task<IReadOnlyList<Delivery>> ListForPurchaseOrderAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Delivery>>([]);

        public Task<int> CountForProjectAsync(
            Guid projectId,
            DeliveryStatus? status,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task AddAsync(
            Delivery delivery,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

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

    private sealed class FakeSupplierRepository(Supplier supplier)
        : ISupplierRepository
    {
        public Task<Supplier?> GetByIdAsync(
            Guid supplierId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Supplier?>(supplier);

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
            Task.FromResult<IReadOnlyList<Supplier>>([supplier]);

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
                    "commercial@example.com",
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
