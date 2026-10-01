# Dashboard

Group 19 introduces live operational dashboards.

## Purpose

The Dashboard is an action-oriented read surface.

It does not create a reporting warehouse, duplicate business state, or persist dashboard totals.

All values are calculated from the existing transactional tables.

Group 20 remains responsible for historical reporting, exports, and reporting-specific views.

## Portfolio dashboard

Routes:

- /
- /dashboard

The portfolio dashboard shows:

- active projects
- closed projects
- overdue active projects
- open site issues
- approval steps currently assigned to the signed-in user
- Purchase Requests pending approval
- Purchase Orders pending approval
- draft Deliveries
- Payment Applications pending approval
- Commercial Certifications pending approval

It also shows up to eight active projects ordered by nearest target completion date.

Each active-project card includes:

- project number
- project name
- location
- target completion date
- overdue status
- open site issue count
- pending approval count
- latest Daily Report date

## Project dashboard

Route:

- /projects/{projectId}/dashboard

Project health includes:

- target completion date
- days remaining or days overdue
- latest Daily Report date and status
- open site issues
- pending Approval Requests

Operational workflow cards show status counts for:

- Daily Reports
- Documents
- Purchase Requests
- Purchase Orders
- Deliveries
- Payment Applications
- Commercial Certifications

Each card links to the underlying operational register when the user has the corresponding permission.

## Authorization

Dashboard access uses:

dashboard.view

The Dashboard does not bypass module authorization.

Navigation from dashboard cards to Projects, Daily Reports, Documents, procurement, commercial, and approval screens remains protected by the existing module permissions.

The bootstrap Administrator automatically receives the new permission through the existing Permissions.All synchronization.

## Read architecture

Dashboard data is exposed through:

Application.Dashboard.IDashboardQueryService

and implemented by:

Infrastructure.Persistence.DashboardQueryService

This is intentionally a read-model service rather than a collection of transactional repositories.

DashboardQueryService uses:

- AsNoTracking queries
- grouped status counts
- aggregate counts
- batched enrichment for active-project cards

It does not issue one repository query per project card.

## Date handling

Dashboard deadline comparisons use the current UTC calendar date supplied through TimeProvider.

Projects are overdue only when:

- Project status is Active
- TargetCompletionDate exists
- TargetCompletionDate is before today

Closed projects are never shown as overdue.

## Persistence

Group 19 adds no tables and no migration.

Dashboard metrics are projections over existing application data.

## Scope boundary

Group 19 deliberately does not add:

- historical trend charts
- earned-value management
- cost forecasting
- cash-flow forecasting
- custom report builders
- Excel/PDF exports
- scheduled reports
- data warehouse tables
- materialized dashboard snapshots

Those belong in Group 20 Reporting or later analytics work.
