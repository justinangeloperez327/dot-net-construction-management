namespace Domain.PurchaseRequests;

public enum PurchaseRequestStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}
