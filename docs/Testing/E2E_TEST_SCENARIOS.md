# EHR Tag Endpoints - E2E Test Scenarios

This document outlines comprehensive end-to-end test scenarios for the EHR tag management system across Patient, Appointment, and Billing services.

## Overview

The tag system provides cross-service tagging capabilities with:
- Tag CRUD operations (create, read, update, archive)
- Tag associations (apply, remove, query)
- Service-based restrictions
- Bulk operations
- Query filtering and search

## Test Execution Environment

- **Framework**: xUnit with IAsyncLifetime for test isolation
- **Database**: SQLite in-memory for realistic EF Core behavior
- **Mocking**: Moq for service interfaces (ITagService, ITagQueryService)
- **Async Pattern**: Full async/await support for realistic async operations

---

## 1. Patient Tag Management Scenarios

### 1.1 Happy Path: Single Tag Application
**Scenario**: Apply a single "VIP" priority tag to a patient
```
Given: Patient ID exists
When: POST /api/v1/patients/{patientId}/tags with tag ID
Then: Tag association created, response returns applied tag ID
And: Tag usage count incremented
And: Audit trail records who applied the tag
```

### 1.2 Multi-Tag Application
**Scenario**: Apply multiple tags simultaneously (VIP + Follow-up + High Risk)
```
Given: Patient ID and 3 valid tag IDs
When: POST /api/v1/patients/{patientId}/tags with array of tag IDs
Then: All 3 tags applied (batch operation)
And: Response includes count of applied tags
And: Total tags on patient resource reflects new count
```

### 1.3 Tag Query
**Scenario**: Retrieve all tags applied to a patient
```
Given: Patient with 3 applied tags
When: GET /api/v1/patients/{patientId}/tags
Then: Response includes all 3 tags with metadata
And: Each tag includes: ID, name, category, color code, description
And: Response can be cached for performance
```

### 1.4 Empty Tag List
**Scenario**: Query patient with no tags
```
Given: Patient ID with no tags applied
When: GET /api/v1/patients/{patientId}/tags
Then: Returns 200 OK with empty array
And: No error thrown for non-existent tags
```

### 1.5 Tag Removal
**Scenario**: Remove a specific tag from patient
```
Given: Patient with "VIP" tag applied
When: DELETE /api/v1/patients/{patientId}/tags/{tagId}
Then: Tag association deleted
And: Returns 204 No Content
And: Tag usage count decremented
```

### 1.6 Replace All Tags
**Scenario**: Set resource to specific tags, removing others
```
Given: Patient with tags [VIP, Reviewed]
When: PUT /api/v1/patients/{patientId}/tags with [High Risk, Follow-up]
Then: Old tags removed, new tags applied
And: Total count updates to 2
And: Response returns new tag list
```

### 1.7 Duplicate Tag Application (Idempotent)
**Scenario**: Apply same tag twice
```
Given: Patient and "VIP" tag
When: POST twice with same tag ID
Then: Second request is idempotent
And: Usage count increments only once
And: No duplicate associations in database
```

### 1.8 Invalid Tag ID (Graceful Failure)
**Scenario**: Apply mix of valid and invalid tags
```
Given: 2 valid tag IDs, 1 non-existent tag ID
When: POST /api/v1/patients/{patientId}/tags with all 3 IDs
Then: 2 tags applied successfully
And: Response indicates "partial success"
And: Error array lists the invalid tag
And: Applied tags count shows 2
```

---

## 2. Appointment Tag Management Scenarios

### 2.1 Appointment Status Tags
**Scenario**: Apply appointment-specific status tags
```
Given: Appointment ID
When: POST /api/v1/appointments/{appointmentId}/tags with [Confirmed, Virtual, Recurring]
Then: All appointment-specific tags applied
And: Context stored (e.g., "Patient confirmed")
```

### 2.2 Appointment Format Tags
**Scenario**: Mark appointment as virtual/in-person
```
Given: New appointment
When: POST /api/v1/appointments/{appointmentId}/tags with Virtual tag
Then: Tag applied with context "Virtual appointment"
And: Can be queried by format tag
```

### 2.3 Appointment Urgent Priority
**Scenario**: Mark appointment as urgent
```
Given: Appointment
When: POST /api/v1/appointments/{appointmentId}/tags with Urgent priority tag
Then: Tag applied with appropriate color code for UI
And: Priority filtering works in list views
```

### 2.4 Query Appointment Tags
**Scenario**: Get all tags on appointment
```
Given: Appointment with [Confirmed, Virtual] tags
When: GET /api/v1/appointments/{appointmentId}/tags
Then: Returns both tags with all metadata
And: UI can display tags with color codes
```

### 2.5 Cancel Appointment (Remove Confirmed Tag)
**Scenario**: Appointment cancelled, "Confirmed" tag removed
```
Given: Appointment with "Confirmed" tag
When: DELETE /api/v1/appointments/{appointmentId}/tags/{confirmedTagId}
Then: Tag removed
And: Appointment status updates
```

### 2.6 Service Restriction - Appointment Only
**Scenario**: Tag restricted to Appointment service only
```
Given: Tag with AllowedServices = "Appointment"
When: POST to apply to appointment
Then: Tag applied successfully
When: Attempt to apply to patient/invoice
Then: Tag denied (service restriction enforced)
```

### 2.7 Bulk Tag Application
**Scenario**: Apply same tag to 50+ appointments
```
Given: 50 appointment IDs, 1 tag ID
When: BulkApplyTagAsync(appointmentIds, appointmentTag)
Then: All 50 tagged efficiently in batch
And: Usage count incremented by 50
```

### 2.8 Concurrent Updates
**Scenario**: Multiple clients update same appointment tags simultaneously
```
Given: 3 concurrent requests to add different tags
When: All POST simultaneously
Then: All succeed without race conditions
And: Final tag count = 3 (all tags applied)
```

---

## 3. Invoice/Billing Tag Management Scenarios

### 3.1 Billing Status Tags
**Scenario**: Apply billing status to invoice
```
Given: Invoice ID
When: POST /api/v1/invoices/{invoiceId}/tags with Paid tag
Then: Tag applied
And: BillingStatus category tracked
```

### 3.2 Payment Method Tags
**Scenario**: Mark payment method on invoice
```
Given: Invoice
When: POST /api/v1/invoices/{invoiceId}/tags with [Insurance, CreditCard]
Then: Both payment method tags applied
And: Can filter invoices by payment method
```

### 3.3 Compliance Tags
**Scenario**: Mark invoice as reviewed/verified
```
Given: Invoice
When: POST /api/v1/invoices/{invoiceId}/tags with [Reviewed, Verified]
Then: Compliance tracking tags applied
And: Audit trail records who reviewed
```

### 3.4 Query Invoice Tags
**Scenario**: Get all tags on invoice
```
Given: Invoice with [Paid, Insurance, Verified] tags
When: GET /api/v1/invoices/{invoiceId}/tags
Then: Returns all 3 tags
And: UI displays billing-specific metadata
```

### 3.5 Dispute Workflow
**Scenario**: Apply dispute tag to invoice
```
Given: Paid invoice
When: POST /api/v1/invoices/{invoiceId}/tags with Disputed tag
Then: Disputed tag applied
When: Query invoice
Then: Disputed tag visible in UI
```

### 3.6 Remove Disputed Tag
**Scenario**: Resolve dispute, remove tag
```
Given: Invoice with [Paid, Disputed] tags
When: DELETE /api/v1/invoices/{invoiceId}/tags/{disputedTagId}
Then: Only Paid tag remains
```

### 3.7 Archived Billing Tag
**Scenario**: Apply archived tag (should fail)
```
Given: Tag with IsArchived = true
When: POST /api/v1/invoices/{invoiceId}/tags with archived tag
Then: Request fails
And: Error indicates tag is archived
And: Tag not applied to invoice
```

### 3.8 Usage Count Tracking
**Scenario**: Track usage across multiple invoices
```
Given: 100 invoices, 1 common tag "BulkPayment"
When: Apply tag to all 100 invoices
Then: UsageCount on tag = 100
And: Denormalized count kept in sync
```

---

## 4. Cross-Service Tag Scenarios

### 4.1 Tag Category Isolation
**Scenario**: Same tag name, different categories
```
Given: Tag "Review" in category "Status"
And: Tag "Review" in category "Document"
When: Query tags
Then: Both visible
And: Can filter by category
```

### 4.2 Service Namespace Isolation
**Scenario**: Same resource ID in different services
```
Given: Guid X tagged in Patient service
And: Same Guid X tagged in Appointment service (different resource types)
When: Query Patient tags
Then: Only patient tags returned
When: Query Appointment tags
Then: Only appointment tags returned
And: No cross-service data leakage
```

### 4.3 Tag Slug Uniqueness
**Scenario**: Tag slug is unique identifier
```
Given: Tag "High Priority" with slug "high-priority"
When: Query by slug
Then: Single tag returned
And: URL-safe access available
```

### 4.4 Color Code for UI Rendering
**Scenario**: Tags have color codes for UI
```
Given: Tags with color codes (#FF5733, #00A86B, etc.)
When: Retrieve tags
Then: Color codes included in response
And: UI renders tags with specified colors
```

### 4.5 System Tag Read-Only
**Scenario**: System-managed tags cannot be modified
```
Given: Tag with IsSystemTag = true
When: Attempt to update tag name
Then: Request denied
And: Error indicates tag is system-managed
```

### 4.6 Tag Description for Documentation
**Scenario**: Tags have human-readable descriptions
```
Given: Tag with description "Requires follow-up within 1 week"
When: Retrieve tag metadata
Then: Description included
And: UI can display tooltip/help text
```

---

## 5. Query and Search Scenarios

### 5.1 Search Tags by Name
**Scenario**: Full-text search tags
```
Given: Tags matching "follow"
When: SearchTagsAsync("follow")
Then: Returns "Follow-up", "Follow Back" tags
And: Search is case-insensitive
```

### 5.2 Filter by Category
**Scenario**: Get all tags in "Priority" category
```
Given: Categories: Priority, Status, Health, Format
When: GetTagsByCategoryAsync("Priority")
Then: Returns [VIP, Urgent, Low, Normal]
```

### 5.3 Filter by Service
**Scenario**: Get tags usable by Patient service
```
Given: Tags with various service restrictions
When: GetTagsByServiceAsync("Patient")
Then: Returns only Patient-available tags
And: Excludes service-restricted tags
```

### 5.4 Include/Exclude Archived Tags
**Scenario**: Query with archive filter
```
Given: 10 active tags, 3 archived tags
When: GetAllAsync(includeArchived: false)
Then: Returns 10 tags
When: GetAllAsync(includeArchived: true)
Then: Returns 13 tags
```

### 5.5 Tag Usage Statistics
**Scenario**: Analytics on tag usage
```
Given: Tags applied to various resources
When: GetTagUsageAsync()
Then: Returns usage statistics for each tag
And: Includes usage count and percentage
And: Shows last applied date
```

### 5.6 Popular Tags
**Scenario**: Get most-used tags for a service
```
Given: Service with multiple tags
When: GetPopularTagsAsync("Patient", limit: 10)
Then: Returns top 10 most-used tags
And: Ordered by usage count descending
```

### 5.7 Recently Applied Tags
**Scenario**: Get recently applied tags
```
Given: Tags applied over time
When: GetRecentlyAppliedTagsAsync(limit: 20)
Then: Returns 20 most recently applied tags
And: Includes which resources were tagged
```

---

## 6. Error Handling & Edge Cases

### 6.1 Non-Existent Patient ID
**Scenario**: Apply tags to non-existent patient
```
When: POST /api/v1/patients/{invalidId}/tags
Then: Can still create association (ref integrity depends on data model)
Or: Returns 404 if enforced
```

### 6.2 Empty Tag ID Array
**Scenario**: POST with no tags
```
When: POST /api/v1/patients/{patientId}/tags with empty TagIds array
Then: Returns 400 Bad Request
And: Error: "No tags provided"
```

### 6.3 Null/Missing Resource Type
**Scenario**: Apply tag with missing resource type
```
When: POST /api/v1/patients/{patientId}/tags with no ResourceType
Then: Controller enforces ResourceType = "PatientEntity"
And: Request still succeeds
```

### 6.4 Concurrent Deletion
**Scenario**: Delete tag while applying it
```
Given: Request A applying tag, Request B deleting tag
When: Execute concurrently
Then: One succeeds, one fails gracefully
And: No orphaned records
```

### 6.5 Database Connection Loss
**Scenario**: Network outage during tag operation
```
When: Database unavailable
Then: Operation fails with 503 Service Unavailable
And: Client can retry
And: No partial data written
```

---

## 7. Performance & Load Scenarios

### 7.1 Bulk Tag Application (1000+ tags)
**Scenario**: Apply 1000 tags to a single patient
```
When: ApplyTagsCommand with 1000 tag IDs
Then: Operation completes in <5 seconds
And: Database efficiently batches inserts
```

### 7.2 High Concurrency (100 concurrent requests)
**Scenario**: 100 clients simultaneously tagging
```
When: 100 concurrent POST requests to same patient
Then: All succeed without deadlock
And: Final state is consistent
```

### 7.3 Large Result Set Query (10000+ resources)
**Scenario**: Query all invoices with "Paid" tag
```
Given: 10000+ invoices tagged as "Paid"
When: SearchResourcesByTagsAsync(tagId, resourceType: "Invoice")
Then: Query completes with pagination
And: Supports limit/offset parameters
```

### 7.4 Tag Usage Count Accuracy
**Scenario**: Verify usage count remains accurate after 10000 operations
```
When: Apply/remove tags repeatedly
Then: UsageCount stays accurate
And: Denormalization does not drift
```

---

## 8. Audit & Compliance Scenarios

### 8.1 Audit Trail Creation
**Scenario**: Every tag operation recorded
```
When: Apply tag to patient
Then: AuditEntry created with:
- Operation type: TagApplied
- Resource: PatientEntity
- Tag ID and Name
- Timestamp
- User ID (AppliedBy)
```

### 8.2 Who Applied Tag
**Scenario**: Track which user applied tag
```
When: POST /api/v1/patients/{id}/tags with AppliedBy = "user123"
Then: TagAssociation.AppliedBy = "user123"
And: Audit trail shows "user123" applied tag
```

### 8.3 Soft Delete Compliance
**Scenario**: Soft-deleted tags not returned
```
When: Tag is deleted (soft delete)
Then: Tag not returned in queries (default)
When: Query with IgnoreQueryFilters()
Then: Deleted tag visible (admin only)
```

---

## 9. Integration Test Structure

### Test Naming Convention
```
[OperationType]_[ResourceType]_[Scenario]_[ExpectedOutcome]

Examples:
- ApplyTag_Patient_SingleTag_SuccessfullyApplied
- RemoveTag_Appointment_NonExistent_ReturnsNotFound
- SetTags_Invoice_ReplaceAll_UpdatesSuccessfully
```

### Test Organization
```
PatientTagsIntegrationTests: 8 tests
  - 3 happy path tests (single tag, multiple tags, query)
  - 2 edge case tests (duplicate, invalid tag)
  - 1 removal test
  - 1 replacement test
  - 1 concurrency test

AppointmentTagsIntegrationTests: 8 tests
  - 3 happy path tests
  - 1 service restriction test
  - 1 bulk operation test
  - 1 non-existent tag removal test
  - 1 replacement test
  - 1 concurrency test

InvoiceTagsIntegrationTests: 8 tests
  - 3 happy path tests
  - 1 invalid resource type test
  - 1 archived tag test
  - 1 usage tracking test
  - 1 replacement test
  - 1 concurrency test
```

### Test Database Setup
- **Type**: SQLite in-memory (more realistic than InMemoryDatabase)
- **Initialization**: IAsyncLifetime.InitializeAsync()
- **Cleanup**: IAsyncLifetime.DisposeAsync()
- **Isolation**: Each test gets fresh database

### Mock Strategy
- **ITagService**: Mocked for command-side operations
- **ITagQueryService**: Mocked for query-side operations
- **Mediator**: Real instance with registered handlers
- **DbContext**: Real in-memory context for data verification

---

## 10. Compilation & Validation

### Build Verification
```bash
# Compile all integration tests
dotnet build backend/tests/EHRPlatform.Tests.Integration

# Verify project references
# - EHRPlatform.Common
# - EHRPlatform.Services.Patient
# - EHRPlatform.Services.Appointment
# - EHRPlatform.Services.Billing
```

### Test Execution
```bash
# Run all integration tests
dotnet test backend/tests/EHRPlatform.Tests.Integration -v normal

# Run specific test class
dotnet test backend/tests/EHRPlatform.Tests.Integration::EHRPlatform.Tests.Integration.Features.Tags.PatientTagsIntegrationTests

# Run with coverage
dotnet test backend/tests/EHRPlatform.Tests.Integration /p:CollectCoverage=true
```

---

## Summary

**Total Test Scenarios**: 30+
- **Patient Tests**: 8 comprehensive scenarios
- **Appointment Tests**: 8 comprehensive scenarios
- **Billing/Invoice Tests**: 8 comprehensive scenarios
- **Cross-service**: 5+ scenarios
- **Query & Search**: 7+ scenarios
- **Error Handling**: 5+ scenarios
- **Performance**: 4+ scenarios
- **Audit & Compliance**: 3+ scenarios

**Coverage**: All tag operations across services with happy paths, edge cases, error conditions, and concurrency scenarios.
