# Approval workflow

Group 12 introduces the reusable approval engine that later business modules can call.

## Scope

An approval request records:

- optional project
- subject type
- subject ID
- reference
- title
- description
- requester
- request timestamp
- request status
- ordered approval steps

A subject is identified by:

```text
SubjectType + SubjectId
```

Examples future modules may use:

```text
PurchaseRequest + <purchase-request-id>
PurchaseOrder + <purchase-order-id>
PaymentApplication + <payment-application-id>
```

The generic engine deliberately does not create database foreign keys to arbitrary subject tables. The module creating the approval request remains responsible for validating that its subject exists.

## Sequential steps

Each step records:

- step number
- step name
- assigned user
- status
- decision timestamp
- comments

Example:

```text
1. Project Engineer
2. Project Manager
3. Commercial Manager
4. General Manager
```

Only the first step begins as Pending. Later steps remain Waiting.

When the current approver approves:

```text
Current step -> Approved
Next step    -> Pending
```

When the final step approves:

```text
ApprovalRequest -> Approved
```

## Rejection

Only the assigned user for the current Pending step can reject.

Rejection requires comments.

When rejected:

```text
Current step    -> Rejected
Later steps     -> Cancelled
ApprovalRequest -> Rejected
```

## Cancellation

Only the original requester can cancel a Pending request.

Outstanding steps become Cancelled.

## Access

The approval inbox is participant scoped.

A user can see a request when they:

- created it; or
- are assigned to at least one of its approval steps.

The detail handler applies the same participant check so knowing an approval request ID does not grant access.

## Permissions

```text
approvals.view
approvals.create
approvals.decide
approvals.cancel
```

The generic creation handler exists for feature modules. Group 12 intentionally does not expose a generic "create arbitrary approval" page.

Purchase Requests and later modules should call the handler with their own validated subject and approval chain.

## Routes

```text
/approvals
/approvals/{approvalRequestId}
```

## Persistence

Tables:

```text
ApprovalRequests
ApprovalSteps
```

Important relationships:

- optional Project foreign key
- Requester foreign key to Identity user
- Approval Step foreign key to Approval Request
- Approver foreign key to Identity user
- unique ApprovalRequestId + StepNumber

A pending-duplicate check exists at the Application layer for the same SubjectType + SubjectId.

Database-level race-condition protection for simultaneous duplicate submissions belongs with the later concurrency/security hardening work.

## Deliberately deferred

Group 12 does not introduce:

- approval workflow templates
- role-based dynamic resolution
- parallel steps
- "any one of N approvers"
- delegation
- automatic escalation
- due dates / service-level timers
- email notifications
- graphical workflow design

These are easy to overbuild before real business rules exist.
