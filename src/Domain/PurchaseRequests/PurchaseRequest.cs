namespace Domain.PurchaseRequests;

public sealed class PurchaseRequest
{
    private readonly List<PurchaseRequestItem> _items = [];

    private PurchaseRequest()
    {
    }

    private PurchaseRequest(
        Guid projectId,
        string requestNumber,
        string title,
        DateOnly? requiredByDate,
        string? purpose,
        Guid requestedByUserId,
        DateTimeOffset createdAt)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "Project ID is required.",
                nameof(projectId));
        }

        if (requestedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Requested-by user ID is required.",
                nameof(requestedByUserId));
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        RequestedByUserId = requestedByUserId;
        CreatedAt = createdAt;
        Status = PurchaseRequestStatus.Draft;

        SetHeader(
            requestNumber,
            title,
            requiredByDate,
            purpose);
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string RequestNumber { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public DateOnly? RequiredByDate { get; private set; }

    public string? Purpose { get; private set; }

    public PurchaseRequestStatus Status { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? ApprovalRequestId { get; private set; }

    public IReadOnlyCollection<PurchaseRequestItem> Items => _items;

    public static PurchaseRequest Create(
        Guid projectId,
        string requestNumber,
        string title,
        DateOnly? requiredByDate,
        string? purpose,
        Guid requestedByUserId,
        DateTimeOffset createdAt)
    {
        return new PurchaseRequest(
            projectId,
            requestNumber,
            title,
            requiredByDate,
            purpose,
            requestedByUserId,
            createdAt);
    }

    public void UpdateHeader(
        string requestNumber,
        string title,
        DateOnly? requiredByDate,
        string? purpose)
    {
        EnsureEditable();

        SetHeader(
            requestNumber,
            title,
            requiredByDate,
            purpose);
    }

    public PurchaseRequestItem AddItem(
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        EnsureEditable();

        var item = new PurchaseRequestItem(
            Id,
            description,
            quantity,
            unit,
            remarks);

        _items.Add(item);

        return item;
    }

    public void UpdateItem(
        Guid itemId,
        string description,
        decimal quantity,
        string unit,
        string? remarks)
    {
        EnsureEditable();

        FindItem(itemId).Update(
            description,
            quantity,
            unit,
            remarks);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        _items.Remove(FindItem(itemId));
    }

    public void SubmitForApproval(Guid approvalRequestId)
    {
        EnsureEditable();

        if (_items.Count == 0)
        {
            throw new InvalidOperationException(
                "A purchase request must contain at least one item before submission.");
        }

        if (approvalRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "Approval request ID is required.",
                nameof(approvalRequestId));
        }

        ApprovalRequestId = approvalRequestId;
        Status = PurchaseRequestStatus.PendingApproval;
    }

    public void ApplyApprovalOutcome(
        Guid approvalRequestId,
        PurchaseRequestStatus outcome)
    {
        if (Status != PurchaseRequestStatus.PendingApproval)
        {
            throw new InvalidOperationException(
                "Only purchase requests pending approval can receive an approval outcome.");
        }

        if (ApprovalRequestId != approvalRequestId)
        {
            throw new InvalidOperationException(
                "Approval outcome does not match the current approval request.");
        }

        if (outcome == PurchaseRequestStatus.Approved)
        {
            Status = PurchaseRequestStatus.Approved;
            return;
        }

        if (outcome == PurchaseRequestStatus.Rejected)
        {
            Status = PurchaseRequestStatus.Rejected;
            return;
        }

        if (outcome == PurchaseRequestStatus.Draft)
        {
            Status = PurchaseRequestStatus.Draft;
            ApprovalRequestId = null;
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(outcome),
            "Unsupported purchase request approval outcome.");
    }

    public void Cancel()
    {
        if (Status is not PurchaseRequestStatus.Draft
            and not PurchaseRequestStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected purchase requests can be cancelled.");
        }

        Status = PurchaseRequestStatus.Cancelled;
    }

    private PurchaseRequestItem FindItem(Guid itemId) =>
        _items.SingleOrDefault(item => item.Id == itemId)
        ?? throw new InvalidOperationException(
            "Purchase request item was not found.");

    private void SetHeader(
        string requestNumber,
        string title,
        DateOnly? requiredByDate,
        string? purpose)
    {
        requestNumber = requestNumber.Trim();
        title = title.Trim();

        if (string.IsNullOrWhiteSpace(requestNumber))
        {
            throw new ArgumentException(
                "Purchase request number is required.",
                nameof(requestNumber));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Purchase request title is required.",
                nameof(title));
        }

        RequestNumber = requestNumber;
        Title = title;
        RequiredByDate = requiredByDate;
        Purpose = NormalizeOptional(purpose);
    }

    private void EnsureEditable()
    {
        if (Status is not PurchaseRequestStatus.Draft
            and not PurchaseRequestStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected purchase requests can be edited.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
