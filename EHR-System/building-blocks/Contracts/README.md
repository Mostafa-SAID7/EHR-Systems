# Contracts Package

API request/response contracts.

## Contents (15 files)

### Requests (3 files)
- `CreateRequest.cs` - Create operation
- `UpdateRequest.cs` - Update operation
- `SearchRequest.cs` - Search criteria

### Responses (8 files)
- `ApiResponse.cs` - Standard response envelope
- `ApiResponseT.cs` - Generic typed response
- `PaginatedResponse.cs` - Paginated results
- `HealthCheckResponse.cs` - Health check data
- `ComponentHealth.cs` - Component status
- `SystemHealth.cs` - Overall health
- `ErrorDetails.cs` - Error information
- `ValidationErrorResponse.cs` - Validation errors

### DTOs (4 files)
- `BaseDto.cs` - Base DTO
- `AuditDto.cs` - Audit information
- `MetadataDto.cs` - Generic metadata
- `PaginationRequest.cs` - Pagination params

---

## Usage

```csharp
using EHRPlatform.Contracts.Responses;
using EHRPlatform.Contracts.Requests;
```

## Parent

[← Building Blocks](../README.md)
