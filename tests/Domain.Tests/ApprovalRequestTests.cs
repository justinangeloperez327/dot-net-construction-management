using Domain.Approvals;
using Xunit;

namespace Domain.Tests;

public sealed class ApprovalRequestTests
{
    [Fact]
    public void Create_activates_only_first_step()
    {
        var firstApprover = Guid.NewGuid();
        var secondApprover = Guid.NewGuid();

        var request = CreateRequest(
            firstApprover,
            secondApprover);

        Assert.Equal(
            ApprovalStepStatus.Pending,
            request.Steps.Single(step => step.StepNumber == 1).Status);

        Assert.Equal(
            ApprovalStepStatus.Waiting,
            request.Steps.Single(step => step.StepNumber == 2).Status);
    }

    [Fact]
    public void Approving_current_step_activates_next_step()
    {
        var firstApprover = Guid.NewGuid();
        var secondApprover = Guid.NewGuid();
        var request = CreateRequest(
            firstApprover,
            secondApprover);
        var first = request.Steps.Single(
            step => step.StepNumber == 1);

        request.Decide(
            first.Id,
            firstApprover,
            ApprovalStepDecision.Approve,
            DateTimeOffset.UtcNow,
            null);

        Assert.Equal(
            ApprovalStepStatus.Approved,
            first.Status);

        Assert.Equal(
            ApprovalStepStatus.Pending,
            request.Steps.Single(step => step.StepNumber == 2).Status);

        Assert.Equal(
            ApprovalRequestStatus.Pending,
            request.Status);
    }

    [Fact]
    public void Wrong_user_cannot_decide_step()
    {
        var request = CreateRequest(
            Guid.NewGuid(),
            Guid.NewGuid());

        var current = request.CurrentStep!;

        Assert.Throws<InvalidOperationException>(() =>
            request.Decide(
                current.Id,
                Guid.NewGuid(),
                ApprovalStepDecision.Approve,
                DateTimeOffset.UtcNow,
                null));
    }

    [Fact]
    public void Reject_requires_comments_and_cancels_remaining_steps()
    {
        var approver = Guid.NewGuid();
        var request = CreateRequest(
            approver,
            Guid.NewGuid());

        var current = request.CurrentStep!;

        Assert.Throws<ArgumentException>(() =>
            request.Decide(
                current.Id,
                approver,
                ApprovalStepDecision.Reject,
                DateTimeOffset.UtcNow,
                null));

        request.Decide(
            current.Id,
            approver,
            ApprovalStepDecision.Reject,
            DateTimeOffset.UtcNow,
            "Budget exceeds approved amount.");

        Assert.Equal(
            ApprovalRequestStatus.Rejected,
            request.Status);

        Assert.Equal(
            ApprovalStepStatus.Cancelled,
            request.Steps.Single(step => step.StepNumber == 2).Status);
    }

    [Fact]
    public void Only_requester_can_cancel_pending_request()
    {
        var requester = Guid.NewGuid();
        var request = ApprovalRequest.Create(
            null,
            "PurchaseRequest",
            Guid.NewGuid(),
            "PR-001",
            "Purchase request approval",
            null,
            requester,
            DateTimeOffset.UtcNow,
            [new ApprovalStepAssignment(
                "Project Manager",
                Guid.NewGuid())]);

        Assert.Throws<InvalidOperationException>(() =>
            request.Cancel(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null));

        request.Cancel(
            requester,
            DateTimeOffset.UtcNow,
            "No longer required.");

        Assert.Equal(
            ApprovalRequestStatus.Cancelled,
            request.Status);

        Assert.All(
            request.Steps,
            step => Assert.Equal(
                ApprovalStepStatus.Cancelled,
                step.Status));
    }

    private static ApprovalRequest CreateRequest(
        Guid firstApprover,
        Guid secondApprover) =>
        ApprovalRequest.Create(
            Guid.NewGuid(),
            "PurchaseRequest",
            Guid.NewGuid(),
            "PR-001",
            "Purchase request approval",
            null,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [
                new ApprovalStepAssignment(
                    "Project Manager",
                    firstApprover),
                new ApprovalStepAssignment(
                    "Commercial Manager",
                    secondApprover)
            ]);
}
