namespace Domain.PaymentApplications;

public enum PaymentApplicationStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}
