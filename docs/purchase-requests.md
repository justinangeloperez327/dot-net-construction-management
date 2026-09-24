# Purchase Requests

Group 14 introduces project-scoped Purchase Requests and connects them to the reusable approval engine.

## Model

A Purchase Request records what a project needs before supplier selection and final commercial commitment.

Header fields:
- project
- PR number
- title
- required-by date
- purpose / justification
- requester
- creation timestamp
- current status
- current or latest approval request

The PR number is unique within a project.

Line items contain:
- description
- quantity
- unit
- remarks

Quantity uses SQL decimal(18,3).

## Workflow

Draft -> Pending Approval -> Approved or Rejected.

Rejected requests can be edited and resubmitted.

Draft or Rejected requests may be cancelled.

Pending Approval and Approved requests are locked.

A PR must have at least one item before submission.

## Approval integration

Submission creates a Group 12 ApprovalRequest with SubjectType PurchaseRequest and SubjectId equal to the Purchase Request ID.

Approvers are explicitly selected in sequence at submission time.

Final approval changes the PR to Approved.
Rejection changes it to Rejected.
Cancelling the approval request returns the PR to Draft.
A resubmission creates a new ApprovalRequest and preserves the previous approval history.

## Atomic persistence

Group 14 introduces IUnitOfWork because submission changes PurchaseRequest and ApprovalRequest in one logical transaction.

The abstraction is intentionally narrow and only commits the shared Entity Framework Core ApplicationDbContext.

## Project closure

Closed projects reject new PRs, edits, item changes, submission, and cancellation. Historical records remain readable.

## Permissions

purchase_requests.view
purchase_requests.create
purchase_requests.update
purchase_requests.submit
purchase_requests.cancel

Approval decisions continue to use approvals.view, approvals.decide, and approvals.cancel.

## Routes

/projects/{projectId}/purchase-requests
/projects/{projectId}/purchase-requests/create
/purchase-requests/{purchaseRequestId}

## Persistence

Tables:
- PurchaseRequests
- PurchaseRequestItems

Important constraints:
- unique ProjectId + RequestNumber
- requester foreign key to Identity
- optional ApprovalRequest foreign key
- Project foreign key
- line items cascade-delete with their Purchase Request

## Deferred

Group 14 does not add supplier selection, quotations, negotiated rates, taxes, final purchase value, Purchase Order numbering, or delivery tracking.
