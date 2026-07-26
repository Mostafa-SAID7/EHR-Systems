# Healthcare Integrations — Claims, Prior Authorization, FHIR & Fraud Detection

## Overview

This guide details the core healthcare integration modules implemented in `EHRPlatform.Services.Billing`, `EHRPlatform.Services.Clinical`, and `EHRPlatform.Common`.

---

## 1. Insurance Claims Processing

Claims follow the **X12 EDI 837 Standard** lifecycle:

```
Invoice Created ──▶ Submit to Insurance ──▶ Fraud Screening ──▶ Claim Submitted (X12 837)
                                                │
                                                └── Risk > 80 ──▶ Claim Placed OnHold
```

### Key Domain Models
- **[InsuranceClaim.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Services.Billing/Domain/Entities/InsuranceClaim.cs)**: Aggregate tracking `PayerId`, `MemberId`, `GroupNumber`, `PriorAuthorizationNumber`, `Npi`, `FraudScore`, and `ClaimStatus`.
- **[ClaimStatus.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Services.Billing/Domain/Enums/ClaimStatus.cs)**: `Submitted`, `Approved`, `Denied`, `Paid`, `Appealing`, `OnHold`.

---

## 2. Prior Authorization Engine

Certain procedures (surgeries, specialty drugs, advanced imaging) require pre-approval prior to service rendering:

- **[PriorAuthorization.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Services.Billing/Domain/Entities/PriorAuthorization.cs)**: Tracks authorization requests, CPT/ICD codes, validity window (`AuthorizedFromDate` to `AuthorizedToDate`), and `AuthorizationNumber`.
- **[RequestPriorAuthorizationCommand.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Services.Billing/Features/Claims/Commands/RequestPriorAuthorizationCommand.cs)**: CQRS command creating PA requests and publishing outbox events.

---

## 3. Fraud & Anomaly Detection

- **[IFraudDetectionService.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Common/Security/IFraudDetectionService.cs)**: Central security evaluation contract.
- **[FraudDetectionService.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Services.Billing/Infrastructure/Services/FraudDetectionService.cs)**: Rule engine detecting high dollar thresholds (> $10k), excessive CPT unbundling, and unverified provider NPIs.

---

## 4. HL7 FHIR R4 Interoperability

- **[FhirEncounterMapper.cs](file:///c:/Users/cw_14/Downloads/New%20folder%20%285%29/backend/src/EHRPlatform.Services.Clinical/Application/Mappers/FhirEncounterMapper.cs)**: Serializes SOAP notes, ICD-10 diagnoses, and CPT procedures into HL7 FHIR R4 Encounter and Condition JSON bundles.
- **Endpoint**: `GET /api/v1/clinicalnotes/{id}/fhir` returning `application/fhir+json`.
