# Commercial Certifications

Group 18 introduces the commercial certification layer for approved Payment Applications.

## Boundary

The Payment Application is the supplier claim.

The Commercial Certification is the contractor/client-side commercial decision on that claim.

The original claim remains unchanged. Certification records how much of the claim is commercially recognized and what deductions apply.

## One certification per claim

Each approved Payment Application can have one Commercial Certification.

Rejected certifications are edited and resubmitted rather than replaced with competing certificates.

Certificate numbers are unique within a Project.

## Amount model

Claimed Amount is captured from the approved Payment Application when the certification is created.

Certified Amount is entered by the commercial team and cannot exceed Claimed Amount.

Uncertified Amount = Claimed Amount - Certified Amount

Retention Amount = Certified Amount x Retention Percent

Total Deductions =
Retention Amount
+ Advance Payment Recovery
+ Other Deductions

Payable Amount = Certified Amount - Total Deductions

Total deductions cannot exceed Certified Amount.

Payable Amount may be zero. A zero payable certificate can be commercially valid where retention and recoveries consume the entire certified amount.

## Other deductions

Other deductions are separate reasoned lines with:

- description
- amount

They are not compressed into one opaque total.

## Workflow

Draft -> Pending Approval -> Approved or Rejected.

Rejected certifications can be corrected and resubmitted.

Draft and Rejected certifications may be cancelled.

Group 12 Approval is reused with:

SubjectType = CommercialCertification
SubjectId = CommercialCertification.Id
Reference = certificate number

Approval synchronizes the certification status.

An Approved certification represents the finance-ready commercial amount. It does not itself post an accounts-payable transaction or execute payment.

## Project close-out

Commercial certifications may continue after operational project closure.

This supports final account and close-out settlement.

## Permissions

commercial_certifications.view
commercial_certifications.create
commercial_certifications.update
commercial_certifications.submit
commercial_certifications.cancel

## Routes

/projects/{projectId}/commercial-certifications
/payment-applications/{paymentApplicationId}/commercial-certifications/create
/commercial-certifications/{certificationId}

## Persistence

Tables:

- CommercialCertifications
- CommercialDeductions

Important constraints:

- unique PaymentApplicationId
- unique ProjectId + CertificateNumber
- Project foreign key
- Payment Application foreign key
- creator Identity foreign key
- optional Approval Request foreign key
- deduction lines cascade with the certification
- business references use restrictive delete behavior

## Deliberately deferred

Group 18 does not add:

- supplier invoice posting
- accounts-payable ledger entries
- payment execution
- payment date
- bank reference
- cheque / transfer reference
- retention release workflow
- advance balance schedule
- variation valuation
- final-account settlement

Those are finance/payment execution or deeper contract-management concerns rather than certification itself.
