# Daily report manpower, equipment, and site issues

Group 8 extends the DailyReport aggregate with operational site details.

## Manpower

Each manpower entry records:

- trade / discipline
- contractor / company
- headcount
- optional total man-hours
- remarks

Headcount must be greater than zero. Man-hours cannot be negative.

## Equipment

Each equipment entry records:

- equipment name
- optional identifier / asset number
- quantity
- optional hours used
- remarks

Quantity must be greater than zero. Hours used cannot be negative.

## Site issues

Each site issue records:

- title
- description
- action taken
- status

Current status values:

```text
Open
Resolved
```

Severity and category are intentionally not introduced until the business has a defined classification scheme.

## Aggregate rules

Manpower, equipment, and issues are owned by DailyReport.

They use the same edit rule as report activities:

```text
Draft / Rejected  -> editable
Submitted         -> locked
Approved          -> locked
```

They do not have independent approval workflows.

## Persistence

Tables:

```text
DailyReportManpower
DailyReportEquipment
DailyReportSiteIssues
```

All child records cascade-delete with the parent DailyReport.

The repository loads the aggregate with Entity Framework Core split queries to avoid a Cartesian explosion when multiple child collections are included.

## Authorization

Group 8 reuses:

```text
daily_reports.view
daily_reports.update
```

No extra permission vocabulary is added because these records are part of editing a daily report rather than independent modules.

## Route

```text
/daily-reports/{reportId}/site-resources
```
