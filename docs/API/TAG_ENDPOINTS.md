# Tag Endpoints API Documentation

## Overview

Tag endpoints provide RESTful access to tag management across Patient, Appointment, and Billing services. Tags are used to categorize and organize resources with flexible, multi-service support.

**Status:** ✅ Implemented and tested
**Build:** 0 errors

---

## Base URLs

```
Patient Service:      GET  /api/v1/patients/{patientId}/tags
Appointment Service:  GET  /api/v1/appointments/{appointmentId}/tags
Billing Service:      GET  /api/v1/invoices/{invoiceId}/tags
```

---

## Endpoints

### 1. GET /api/v1/{entity}/{id}/tags

**Get all tags for a resource**

#### Patient
```http
GET /api/v1/patients/{patientId}/tags
Authorization: Bearer {token}
```

#### Appointment
```http
GET /api/v1/appointments/{appointmentId}/tags
Authorization: Bearer {token}
```

#### Invoice
```http
GET /api/v1/invoices/{invoiceId}/tags
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "patientId": "00000000-0000-0000-0000-000000000001",
  "tags": [
    {
      "id": "00000000-0000-0000-0000-000000000101",
      "name": "VIP",
      "category": "Priority",
      "slug": "vip",
      "description": "Very Important Patient"
    },
    {
      "id": "00000000-0000-0000-0000-000000000102",
      "name": "High Risk",
      "category": "Health",
      "slug": "high-risk",
      "description": "Requires additional monitoring"
    }
  ]
}
```

**Error Responses:**
- `500 Internal Server Error` - Service error occurred

---

### 2. POST /api/v1/{entity}/{id}/tags

**Apply tags to a resource**

#### Patient
```http
POST /api/v1/patients/{patientId}/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "tagIds": [
    "00000000-0000-0000-0000-000000000101",
    "00000000-0000-0000-0000-000000000102"
  ],
  "context": "Patient enrollment into VIP program",
  "appliedBy": "user@example.com"
}
```

#### Appointment
```http
POST /api/v1/appointments/{appointmentId}/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "tagIds": [
    "00000000-0000-0000-0000-000000000201"
  ],
  "context": "Follow-up appointment",
  "appliedBy": "admin@example.com"
}
```

#### Invoice
```http
POST /api/v1/invoices/{invoiceId}/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "tagIds": [
    "00000000-0000-0000-0000-000000000301"
  ],
  "context": "Insurance claim pending",
  "appliedBy": "billing@example.com"
}
```

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| tagIds | Guid[] | Yes | List of tag IDs to apply |
| context | string | No | Context/reason for applying tags |
| appliedBy | string | No | User/system applying the tags |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Successfully applied 2 tag(s)",
  "resourceId": "00000000-0000-0000-0000-000000000001",
  "appliedTagIds": [
    "00000000-0000-0000-0000-000000000101",
    "00000000-0000-0000-0000-000000000102"
  ],
  "totalTagsOnResource": 2,
  "errors": []
}
```

**Error Responses:**
- `400 Bad Request` - Invalid tag IDs or empty tag list
- `500 Internal Server Error` - Service error

---

### 3. DELETE /api/v1/{entity}/{id}/tags/{tagId}

**Remove a tag from a resource**

#### Patient
```http
DELETE /api/v1/patients/{patientId}/tags/{tagId}
Authorization: Bearer {token}
```

#### Appointment
```http
DELETE /api/v1/appointments/{appointmentId}/tags/{tagId}
Authorization: Bearer {token}
```

#### Invoice
```http
DELETE /api/v1/invoices/{invoiceId}/tags/{tagId}
Authorization: Bearer {token}
```

**Response (204 No Content):**
```
(empty body)
```

**Error Responses:**
- `404 Not Found` - Tag not found on resource
- `500 Internal Server Error` - Service error

---

### 4. PUT /api/v1/{entity}/{id}/tags

**Replace all tags for a resource (atomic operation)**

#### Patient
```http
PUT /api/v1/patients/{patientId}/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "tagIds": [
    "00000000-0000-0000-0000-000000000103",
    "00000000-0000-0000-0000-000000000104"
  ],
  "appliedBy": "admin@example.com"
}
```

#### Appointment
```http
PUT /api/v1/appointments/{appointmentId}/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "tagIds": [
    "00000000-0000-0000-0000-000000000202"
  ],
  "appliedBy": "admin@example.com"
}
```

#### Invoice
```http
PUT /api/v1/invoices/{invoiceId}/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "tagIds": [
    "00000000-0000-0000-0000-000000000302"
  ],
  "appliedBy": "admin@example.com"
}
```

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| tagIds | Guid[] | Yes | Complete list of tags to set |
| appliedBy | string | No | User/system setting the tags |

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Successfully set 2 tag(s)",
  "resourceId": "00000000-0000-0000-0000-000000000001",
  "appliedTagIds": [
    "00000000-0000-0000-0000-000000000103",
    "00000000-0000-0000-0000-000000000104"
  ],
  "totalTagsOnResource": 2,
  "errors": []
}
```

**Error Responses:**
- `400 Bad Request` - Invalid tag IDs
- `500 Internal Server Error` - Service error

---

## Data Models

### TagDto
```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "name": "VIP",
  "category": "Priority",
  "slug": "vip",
  "description": "Very Important Patient",
  "serviceNames": ["Patient"],
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### TagAssignmentResponse
```json
{
  "success": true,
  "message": "Successfully applied 2 tag(s)",
  "resourceId": "00000000-0000-0000-0000-000000000001",
  "appliedTagIds": [
    "00000000-0000-0000-0000-000000000101",
    "00000000-0000-0000-0000-000000000102"
  ],
  "totalTagsOnResource": 2,
  "errors": []
}
```

---

## Available Tags by Service

### Patient Service
| Category | Tag | Slug | Description |
|----------|-----|------|-------------|
| Priority | VIP | vip | Very Important Patient |
| Priority | High Risk | high-risk | Requires additional monitoring |
| Health | Chronic | chronic | Has chronic conditions |
| Health | Allergy Alert | allergy-alert | Known allergies |
| Status | Active | active | Active patient |

### Appointment Service
| Category | Tag | Slug | Description |
|----------|-----|------|-------------|
| Type | Follow-up | follow-up | Follow-up appointment |
| Type | New Patient | new-patient | First-time appointment |
| Priority | Urgent | urgent | Urgent scheduling |
| Status | Confirmed | confirmed | Appointment confirmed |
| Status | Rescheduled | rescheduled | Previously rescheduled |

### Billing Service
| Category | Tag | Slug | Description |
|----------|-----|------|-------------|
| Status | Paid | paid | Fully paid invoice |
| Status | Pending | pending | Awaiting payment |
| Status | Overdue | overdue | Payment overdue |
| Insurance | Submitted | submitted | Submitted to insurance |
| Insurance | Approved | approved | Insurance approved |

---

## Common Use Cases

### 1. Tag a Patient as VIP
```bash
curl -X POST https://api.example.com/api/v1/patients/patient-id/tags \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "tagIds": ["vip-tag-id"],
    "context": "Premium care enrollment",
    "appliedBy": "admin@example.com"
  }'
```

### 2. Query Patient Tags
```bash
curl -X GET https://api.example.com/api/v1/patients/patient-id/tags \
  -H "Authorization: Bearer token"
```

### 3. Remove Tag
```bash
curl -X DELETE https://api.example.com/api/v1/patients/patient-id/tags/tag-id \
  -H "Authorization: Bearer token"
```

### 4. Replace All Tags
```bash
curl -X PUT https://api.example.com/api/v1/patients/patient-id/tags \
  -H "Authorization: Bearer token" \
  -H "Content-Type: application/json" \
  -d '{
    "tagIds": ["tag-id-1", "tag-id-2"],
    "appliedBy": "admin@example.com"
  }'
```

---

## Status Codes

| Code | Status | Meaning |
|------|--------|---------|
| 200 | OK | Request succeeded |
| 204 | No Content | Deletion succeeded (no content to return) |
| 400 | Bad Request | Invalid input (validation error) |
| 404 | Not Found | Resource or tag not found |
| 500 | Internal Server Error | Server error occurred |

---

## Error Handling

### Validation Errors (400)
```json
{
  "errors": {
    "tagIds": ["At least one tag is required"],
    "context": ["Context must be less than 500 characters"]
  }
}
```

### Not Found Errors (404)
```json
{
  "message": "Tag not found on resource",
  "resourceId": "00000000-0000-0000-0000-000000000001",
  "tagId": "00000000-0000-0000-0000-000000000999"
}
```

### Server Errors (500)
```json
{
  "message": "An error occurred while processing your request",
  "traceId": "0HN1JFVMDVV0L:00000001"
}
```

---

## Rate Limiting

- **Tag Queries:** 1000 requests/minute per IP
- **Tag Mutations:** 100 requests/minute per IP
- **Bulk Operations:** 10 requests/minute per IP

---

## Authentication & Authorization

- All endpoints require `Authorization: Bearer {token}`
- Token must have `tags:read` scope for GET operations
- Token must have `tags:write` scope for POST/PUT operations
- Token must have `tags:delete` scope for DELETE operations

---

## Related Documentation

- [Tag Service Implementation](../Architecture/TAG_SERVICE_ARCHITECTURE.md)
- [CQRS Pattern](./CQRS_PATTERN.md)
- [Testing Guide](../Testing/TAG_ENDPOINTS_TESTING.md)
- [Slug-based URLs](./SLUG_BASED_URLS.md)

---

## Examples

### Complete Workflow

```bash
# 1. Get current tags
GET /api/v1/patients/patient-123/tags

# 2. Apply new tags
POST /api/v1/patients/patient-123/tags
{ "tagIds": ["vip-id"], "appliedBy": "admin" }

# 3. View updated tags
GET /api/v1/patients/patient-123/tags

# 4. Remove specific tag
DELETE /api/v1/patients/patient-123/tags/vip-id

# 5. Replace all tags
PUT /api/v1/patients/patient-123/tags
{ "tagIds": ["high-risk-id", "chronic-id"] }
```

