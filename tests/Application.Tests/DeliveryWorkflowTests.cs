using Application.Common.Authentication;
using Application.Deliveries;
using Application.Projects;
using Application.PurchaseOrders;
using Domain.Deliveries;
using Domain.Projects;
using Domain.PurchaseOrders;
using Xunit;

namespace Application.Tests;

public sealed class DeliveryWorkflowTests
{
    [Fact]
    public async Task Receive_posts_partial_delivery()
    {
        var receiverId = Guid.NewGuid();
        var order = CreateApprovedOrder();
        var orderItem = order.Items.Single();
        var delivery = CreateDelivery(order);

        delivery.AddItem(
            orderItem.Id,
            orderItem.Description,
            4m,
            orderItem.Unit,
            null);

        var deliveries = new FakeDeliveryRepository(delivery);

        var handler = new ReceiveDeliveryHandler(
            deliveries,
            new FakePurchaseOrderRepository(order),
            new FakeProjectRepository(CreateProject()),
            new FakeCurrentUser(receiverId),
            TimeProvider.System);

        var result = await handler.HandleAsync(delivery.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(DeliveryStatus.Received, delivery.Status);
        Assert.Equal(receiverId, delivery.ReceivedByUserId);
        Assert.NotNull(delivery.ReceivedAt);
        Assert.Equal(1, deliveries.SaveCount);
    }

    [Fact]
    public async Task Receive_rejects_quantity_above_remaining_po_balance()
    {
        var order = CreateApprovedOrder();
        var orderItem = order.Items.Single();
        var delivery = CreateDelivery(order);

        delivery.AddItem(
            orderItem.Id,
            orderItem.Description,
            4m,
            orderItem.Unit,
            null);

        var deliveries = new FakeDeliveryRepository(delivery)
        {
            PreviouslyReceivedQuantity = 7m
        };

        var handler = new ReceiveDeliveryHandler(
            deliveries,
            new FakePurchaseOrderRepository(order),
            new FakeProjectRepository(CreateProject()),
            new FakeCurrentUser(Guid.NewGuid()),
            TimeProvider.System);

        var result = await handler.HandleAsync(delivery.Id);

        Assert.False(result.Succeeded);
        Assert.Equal(DeliveryStatus.Draft, delivery.Status);
        Assert.Equal(0, deliveries.SaveCount);
    }

    [Fact]
    public async Task Add_item_rejects_quantity_above_remaining_po_balance()
    {
        var order = CreateApprovedOrder();
        var orderItem = order.Items.Single();
        var delivery = CreateDelivery(order);

        var deliveries = new FakeDeliveryRepository(delivery)
        {
            PreviouslyReceivedQuantity = 8m
        };

        var handler = new AddDeliveryItemHandler(
            deliveries,
            new FakePurchaseOrderRepository(order),
            new FakeProjectRepository(CreateProject()));

        var result = await handler.HandleAsync(
            delivery.Id,
            new DeliveryItemCommand(
                orderItem.Id,
                3m,
                null));

        Assert.False(result.Succeeded);
        Assert.Empty(delivery.Items);
    }

    private static Delivery CreateDelivery(PurchaseOrder order) =>
        Delivery.Create(
            order.ProjectId,
            order.Id,
            "DN-001",
            new DateOnly(2026, 10, 2),
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

    private sealed class FakeDeliveryRepository(Delivery delivery)
        : IDeliveryRepository
    {
        public decimal PreviouslyReceivedQuantity { get; set; }

        public int SaveCount { get; private set; }

        public Task<Delivery?> GetByIdAsync(
            Guid deliveryId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Delivery?>(
                delivery.Id == deliveryId ? delivery : null);

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
            Task.FromResult(PreviouslyReceivedQuantity);

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
            Delivery newDelivery,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
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

    private sealed class FakeCurrentUser(Guid userId)
        : ICurrentUser
    {
        public ValueTask<CurrentUserInfo> GetAsync(
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(
                new CurrentUserInfo(
                    userId,
                    "receiver@example.com",
                    true));
    }
}
