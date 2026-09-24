# Daily site reports

Group 7 adds the daily construction-progress reporting workflow.

## Scope

A daily report contains:

- project
- report date
- preparer
- weather / site conditions
- general remarks
- activity rows
- submission timestamp
- reviewer
- review timestamp
- review comments
- workflow status

Only one report may exist for a project on a given date.

## Activity rows

Each activity records:

- work area / location
- activity description
- status
- optional progress percent
- remarks

Activity status is currently:

```text
InProgress
Completed
OnHold
```

This supports actual site entries such as BS-1 / AC ducting / InProgress without hard-coding project-specific locations such as BS-1 into the model.

## Workflow

```text
Draft
  ↓
Submitted
  ├──→ Approved
  └──→ Rejected
          ↓
       edit/resubmit
```

Rules:

- Draft and Rejected reports are editable.
- Submitted reports are locked while under review.
- Approved reports are immutable.
- A report must have at least one activity before submission.
- Rejection requires reviewer comments.
- Resubmission clears the previous review result.
- Reports cannot be created for closed projects.

## Permissions

```text
daily_reports.view
daily_reports.create
daily_reports.update
daily_reports.submit
daily_reports.review
```

## Routes

```text
/projects/{projectId}/daily-reports
/projects/{projectId}/daily-reports/create
/daily-reports/{reportId}
```

## Deferred to later groups

Group 7 intentionally does not contain:

- manpower entries
- equipment entries
- site issues
- photos or attachments

Manpower, equipment, and site issues are Group 8. File attachments are Group 9.
