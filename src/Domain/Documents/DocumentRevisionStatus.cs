namespace Domain.Documents;

public enum DocumentRevisionStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    ApprovedWithComments = 4,
    Rejected = 5,
    Superseded = 6
}
