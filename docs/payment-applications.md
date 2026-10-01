# Payment Applications

Group 17 introduces supplier payment applications against approved Purchase Orders and received quantities.

## Commercial boundary

A Payment Application records the amount being claimed against work/material already received under an approved PO.

It is not a payment transaction and it is not an invoice-posting module.

The flow is:

Approved PO -> Received deliveries -> Payment Application -> Approval.

Group 18 will add commercial certification rules such as deductions, retention, certified amount, and finance-facing approval decisions.

## Header

A Payment Application records:

- project
- purchase order
- application number
- application date
- optional period from / to
- notes
- creator
- creation timestamp
- status
- current/latest approval request

The application number is unique within a Purchase Order.

## Claim lines

Each line links to a Purchase Order item and copies the approved commercial terms:

- description
- claimed quantity
- unit
- unit price
- discount percent
- tax percent
- remarks

The user enters quantity, not a free-form claimed amount.

Commercial value is derived using the same PO terms.

## Claimability control

Only Received deliveries create claimable quantity.

For each PO line:

claimable quantity = received quantity - committed claim quantity

Committed claim quantity includes Payment Applications with status:

- Pending Approval
- Approved

Draft and Rejected applications do not reserve quantity.

At submission, every line is revalidated to protect against concurrent or later claims consuming the same received balance.

## Commercial calculations

Claimed quantity uses decimal(18,3).

Unit price uses decimal(18,4).

Discount and tax percentages use decimal(5,2).

Gross = Claimed Quantity x Unit Price

Discount Amount = Gross x Discount %

Net = Gross - Discount Amount

Tax Amount = Net x Tax %

Claimed Amount = Net + Tax Amount

Application totals are derived from claim lines rather than persisted separately.

## Workflow

Draft -> Pending Approval -> Approved or Rejected.

Rejected applications can be corrected and resubmitted.

Draft and Rejected applications can be cancelled.

Approval uses the reusable Group 12 engine with:

SubjectType = PaymentApplication
SubjectId = PaymentApplication.Id
Reference = application number

## Project close-out

Payment Applications are intentionally not blocked solely because a Project is Closed.

Commercial settlement, final claims, and close-out frequently continue after operational work is complete.

The controlling prerequisites are:

- Purchase Order remains Approved
- quantity was physically Received
- quantity has not already been committed by another Payment Application

## Permissions

payment_applications.view
payment_applications.create
payment_applications.update
payment_applications.submit
payment_applications.cancel

## Routes

/projects/{projectId}/payment-applications
/purchase-orders/{purchaseOrderId}/payment-applications/create
/payment-applications/{paymentApplicationId}

## Persistence

Tables:

- PaymentApplications
- PaymentApplicationItems

Important constraints:

- unique PurchaseOrderId + ApplicationNumber
- Project foreign key
- PurchaseOrder foreign key
- creator Identity foreign key
- optional ApprovalRequest foreign key
- PaymentApplicationItem foreign key to PurchaseOrderItem
- unique PaymentApplicationId + PurchaseOrderItemId
- child lines cascade with their application
- referenced procurement records use restrictive delete behavior

## Deliberately deferred to commercial/finance work

Group 17 does not add:

- certified amount
- retention
- advance-payment recovery
- other deductions
- variation valuation
- invoice number / invoice posting
- accounts-payable posting
- payment execution
- bank transaction reference
- three-way invoice matching

Those require explicit commercial and finance rules rather than arbitrary fields.
