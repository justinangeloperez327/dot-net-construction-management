# Purchase Orders

Group 15 introduces commercial Purchase Orders created from approved Purchase Requests.

## Procurement boundary

A Purchase Request describes what the project needs.

A Purchase Order records the supplier and committed commercial terms.

The flow is:

Approved Purchase Request -> Draft Purchase Order -> Approval -> Approved Purchase Order.

## Header

A Purchase Order records:

- project
- source Purchase Request
- supplier
- PO number
- order date
- expected delivery date
- currency
- delivery address
- delivery terms
- payment terms
- notes
- creator
- creation timestamp
- status
- current or latest approval request

The PO number is unique across the company.

Currency is stored as a normalized three-letter code.

## Lines

When a PO is created, approved PR lines are copied into the draft PO.

Each PO line preserves a reference to its source PR line and records:

- description
- quantity
- unit
- unit price
- discount percent
- tax percent
- remarks

A buyer can reduce quantities or remove lines before submission. This enables split awards across suppliers.

A PO line cannot exceed its source PR line quantity.

## Split-award control

Draft and Rejected POs do not reserve PR quantity.

At submission, the application calculates quantity already committed by other POs whose status is:

- Pending Approval
- Approved

For each source PR line:

committed quantity + current PO quantity <= approved PR quantity

This prevents multiple supplier awards from over-ordering the approved requisition quantity.

Cancelled POs do not consume allocation.

## Commercial calculations

Quantity uses decimal(18,3).

Unit price uses decimal(18,4).

Discount and tax percentages use decimal(5,2).

Line calculations are:

Gross = Quantity x Unit Price

Discount Amount = Gross x Discount %

Net = Gross - Discount Amount

Tax Amount = Net x Tax %

Line Total = Net + Tax Amount

PO Subtotal, Discount Total, Tax Total, and Grand Total are derived from the line items. They are not duplicated as persisted header totals.

A PO must have at least one line and a Grand Total greater than zero before submission.

## Workflow

Draft -> Pending Approval -> Approved or Rejected.

Rejected POs can be corrected and resubmitted.

Draft and Rejected POs may be cancelled.

Approved POs are immutable in Group 15. Post-award amendments and approved-PO cancellation require explicit commercial rules and are deliberately deferred.

## Approval integration

Group 15 reuses the Group 12 approval engine with:

SubjectType = PurchaseOrder
SubjectId = PurchaseOrder.Id
Reference = PO number

Final approval changes the PO to Approved.
Rejection changes the PO to Rejected.
Cancelling a pending approval returns the PO to Draft.

## Supplier rules

Only active suppliers can be selected or submitted.

The PO retains its Supplier foreign key so later supplier deactivation does not remove procurement history.

## Project rules

Closed projects reject:

- PO creation
- header edits
- line edits/removal
- PO submission
- draft/rejected cancellation

Historical POs remain readable.

## Permissions

purchase_orders.view
purchase_orders.create
purchase_orders.update
purchase_orders.submit
purchase_orders.cancel

Approval decisions continue to use approvals.view, approvals.decide, and approvals.cancel.

## Routes

/projects/{projectId}/purchase-orders
/purchase-requests/{purchaseRequestId}/purchase-orders/create
/purchase-orders/{purchaseOrderId}

## Persistence

Tables:

- PurchaseOrders
- PurchaseOrderItems

Important constraints include:

- company-wide unique PurchaseOrderNumber
- Project foreign key
- approved-source PurchaseRequest foreign key
- Supplier foreign key
- creator Identity foreign key
- optional ApprovalRequest foreign key
- PO item source PurchaseRequestItem foreign key
- PO items cascade with their PO
- referenced business records use restrictive delete behavior

## Deferred

Group 15 does not add:

- Request For Quotation and quotation comparison
- Purchase Order amendments / change orders
- approved PO cancellation workflow
- supplier acknowledgements
- delivery receipts
- invoice matching
- payment applications

Delivery execution is Group 16.
