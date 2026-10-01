# Deliveries

Group 16 introduces receiving against approved Purchase Orders.

## Scope

A Delivery represents one supplier delivery note received against one approved PO.

Header fields:
- project
- purchase order
- supplier delivery note number
- delivery date
- vehicle / transport reference
- remarks
- creator
- creation timestamp
- status
- received-by user
- received timestamp

The delivery note number is unique within a Purchase Order.

## Items

Each delivery item links directly to a Purchase Order item and records:
- description snapshot
- delivered quantity
- unit snapshot
- remarks

Quantity uses decimal(18,3), matching the Purchase Order quantity precision.

## Workflow

Draft -> Received

Draft -> Cancelled

Only Draft deliveries can be edited or cancelled.

A Received delivery is immutable. Group 16 does not allow editing or cancelling a posted receipt because doing so would imply a stock or quantity reversal workflow.

## Partial deliveries

A PO line can be fulfilled through multiple deliveries.

Only deliveries with status Received count toward cumulative receiving.

For each PO line:

previously received + current delivery quantity <= ordered quantity

Draft deliveries do not reserve quantity.

This lets users prepare a draft receipt without blocking another actual delivery, while the Receive action revalidates quantities before posting.

## Controls

Deliveries can only be created and received against Approved Purchase Orders.

Closed projects reject:
- new deliveries
- header edits
- line changes
- receipt posting
- draft cancellation

Delivery date cannot be before the Purchase Order date.

## Permissions

deliveries.view
deliveries.create
deliveries.update
deliveries.receive
deliveries.cancel

## Routes

/projects/{projectId}/deliveries
/purchase-orders/{purchaseOrderId}/deliveries/create
/deliveries/{deliveryId}

## Persistence

Tables:
- Deliveries
- DeliveryItems

Important constraints:
- unique PurchaseOrderId + DeliveryNoteNumber
- Project foreign key
- PurchaseOrder foreign key
- creator Identity foreign key
- receiver Identity foreign key
- DeliveryItem foreign key to PurchaseOrderItem
- unique DeliveryId + PurchaseOrderItemId
- DeliveryItems cascade with their Delivery
- referenced procurement records use restrictive delete behavior

## Deliberately deferred

Group 16 does not add:
- warehouse stock balances
- goods receipt valuation
- quality inspection / Material Inspection Request
- damaged or rejected material workflow
- return-to-supplier workflow
- delivery attachments
- supplier acknowledgement
- invoice matching
- three-way matching
- approved receipt reversal

Those features require their own business rules rather than mutating a historical receipt.
