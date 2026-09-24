using Domain.PurchaseRequests;
using Xunit;

namespace Domain.Tests;

public sealed class PurchaseRequestTests
{
    [Fact]
    public void Submit_requires_at_least_one_item()
    {
        var request = CreateRequest();

        Assert.Throws<InvalidOperationException>(() =>
            request.SubmitForApproval(Guid.NewGuid()));
    }

    [Fact]
    public void Pending_request_is_locked()
    {
        var request = CreateRequest();
        request.AddItem("Cement board", 10, "pcs", null);
        request.SubmitForApproval(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            request.AddItem("Screws", 100, "pcs", null));
    }

    [Fact]
    public void Rejected_request_can_be_corrected_and_resubmitted()
    {
        var request = CreateRequest();
        request.AddItem("Cement board", 10, "pcs", null);

        var firstApprovalId = Guid.NewGuid();
        request.SubmitForApproval(firstApprovalId);
        request.ApplyApprovalOutcome(
            firstApprovalId,
            PurchaseRequestStatus.Rejected);

        request.AddItem("Screws", 100, "pcs", null);

        var secondApprovalId = Guid.NewGuid();
        request.SubmitForApproval(secondApprovalId);

        Assert.Equal(
            PurchaseRequestStatus.PendingApproval,
            request.Status);
        Assert.Equal(secondApprovalId, request.ApprovalRequestId);
        Assert.Equal(2, request.Items.Count);
    }

    [Fact]
    public void Cancelled_approval_returns_request_to_draft()
    {
        var request = CreateRequest();
        request.AddItem("Cement board", 10, "pcs", null);

        var approvalId = Guid.NewGuid();
        request.SubmitForApproval(approvalId);
        request.ApplyApprovalOutcome(
            approvalId,
            PurchaseRequestStatus.Draft);

        Assert.Equal(PurchaseRequestStatus.Draft, request.Status);
        Assert.Null(request.ApprovalRequestId);
    }

    [Fact]
    public void Approved_request_cannot_be_cancelled()
    {
        var request = CreateRequest();
        request.AddItem("Cement board", 10, "pcs", null);

        var approvalId = Guid.NewGuid();
        request.SubmitForApproval(approvalId);
        request.ApplyApprovalOutcome(
            approvalId,
            PurchaseRequestStatus.Approved);

        Assert.Throws<InvalidOperationException>(request.Cancel);
    }

    private static PurchaseRequest CreateRequest() =>
        PurchaseRequest.Create(
            Guid.NewGuid(),
            "PR-001",
            "Site materials",
            new DateOnly(2026, 10, 1),
            "Required for ongoing works.",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);
}
