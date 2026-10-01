# Reporting

Group 20 adds historical and exportable reporting over the existing construction-management data.

## Responsibility

The Dashboard from Group 19 answers:

- What needs attention now?
- What is the current project/workflow state?

Reporting answers:

- What records exist over a selected period?
- What is the historical register?
- What data should be exported for review, reconciliation, or downstream analysis?

Reporting does not create duplicate business state.

## Report workspace

Route:

- /reports

The workspace provides a shared filter:

- Project
- From Date
- To Date

The date filter applies to:

- Daily Site Register
- Procurement Register
- Commercial Register

The Project Portfolio is a current project register and ignores the date range.

A project-specific shortcut is available from the Project Overview:

/reports?projectId={projectId}

## Report families

### Project Portfolio

Fields include:

- Project Number
- Project Name
- Client
- Location
- Status
- Start Date
- Target Completion
- Latest Daily Report
- Open Site Issues
- Pending Approvals

### Daily Site Register

Fields include:

- Project
- Report Date
- Status
- Prepared By
- Weather
- Activity Count
- Manpower Headcount
- Equipment Quantity
- Open Site Issues

### Procurement Register

The procurement report is Purchase Order based.

Fields include:

- Project
- Purchase Request Number
- Purchase Order Number
- Supplier
- PO Status
- Order Date
- Expected Delivery Date
- Currency
- Purchase Order Total
- number of Received deliveries
- Latest Received Delivery date

Only deliveries with status Received count as received deliveries.

### Commercial Register

The commercial report is Payment Application based.

Fields include:

- Project
- Purchase Order
- Supplier
- Payment Application
- Application Date
- Application Status
- Currency
- Claimed Amount
- Commercial Certificate Number
- Certificate Date
- Certification Status
- Certified Amount
- Payable Amount

Payment Applications without a Commercial Certification remain visible.

## Screen limits

Each on-screen report is limited to 200 rows by default.

The UI displays:

Showing 200 of N rows. Export CSV for the full filtered result.

This prevents the Blazor page from rendering an unbounded table.

CSV exports request the full filtered dataset.

## CSV export

Permission-protected routes:

- /reports/export/portfolio
- /reports/export/dailysite
- /reports/export/procurement
- /reports/export/commercial

Supported query parameters:

- projectId
- from
- to

Example:

/reports/export/procurement?projectId={guid}&from=2026-09-01&to=2026-09-30

CSV behavior:

- UTF-8 with BOM for Excel compatibility
- invariant date format yyyy-MM-dd
- invariant decimal formatting
- RFC-style escaping for comma, quote, carriage return, and newline
- same filters and read service as the on-screen report

The export endpoint rejects a date range where To is before From.

## Authorization

Permissions:

reports.view
reports.export

Viewing reports and downloading data are deliberately separate permissions.

The existing bootstrap Administrator receives both permissions through Permissions.All synchronization.

## Read architecture

Application:

- Application.Reporting.IReportingQueryService
- report row models
- report query handlers
- ExportReportCsvHandler

Infrastructure:

- Infrastructure.Persistence.ReportingQueryService

The reporting service uses:

- AsNoTracking
- filtered and ordered EF Core queries
- grouped child-record counts
- batched enrichment of project/supplier/user lookups
- includes only where aggregate business calculations require child lines

Reporting remains outside the Domain layer.

## Persistence

Group 20 adds no tables and no migration.

Reports are live projections over existing transactional data.

## Scope boundary

Group 20 deliberately does not add:

- PDF rendering
- custom report designer
- scheduled report emails
- report subscriptions
- data warehouse tables
- cubes / OLAP
- cross-currency financial aggregation
- earned-value management
- cash-flow forecasting
- business-intelligence dashboards

Those require dedicated business definitions and/or a later analytics architecture.
