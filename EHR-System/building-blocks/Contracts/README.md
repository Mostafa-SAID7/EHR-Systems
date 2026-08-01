# Contracts Package

Cross-service API contracts, DTOs, requests, and responses for microservices communication.

## Folder Structure

```
Contracts/
├── src/
│   ├── Dto/
│   │   ├── BaseDto.cs ..................... Audit trail DTO base
│   │   └── Healthcare/
│   │       └── PatientDto.cs ............. Patient DTO
│   ├── Requests/
│   │   ├── CreateRequest.cs .............. POST/Create request base
│   │   ├── UpdateRequest.cs .............. PUT/PATCH request base
│   │   └── SearchRequest.cs .............. GET list/search with pagination
│   ├── Responses/
│   │   ├── ApiResponse.cs ................ Standard API response envelope
│   │   ├── PaginatedResponse.cs .......... Paginated list response
│   │   └── HealthCheckResponse.cs ........ Service health check response
│   └── Contracts.csproj
│
└── tests/
    ├── Unit/
    │   └── (test files here)
    └── Contracts.Tests.csproj
```

## Single Responsibility Organization

### DTOs (Data Transfer Objects)
- **BaseDto.cs** - Audit trail fields (Id, CreatedAt, UpdatedAt, etc.)
- **Healthcare/PatientDto.cs** - Domain-specific patient data

### Requests
- **CreateRequest.cs** - POST operation contracts only
- **UpdateRequest.cs** - PUT/PATCH operation contracts only
- **SearchRequest.cs** - GET list operations with pagination & sorting

### Responses
- **ApiResponse.cs** - Standard response envelope (success/failure with metadata)
- **PaginatedResponse.cs** - Pagination metadata for list responses
- **HealthCheckResponse.cs** - Service health check responses

## Usage Examples

### Request Contracts

```csharp
// Create request
public class CreatePatientRequest : CreateRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// Update request
public class UpdatePatientRequest : UpdateRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Search request
public class SearchPatientRequest : SearchRequest
{
    public string? Status { get; set; }
    
    // Inherited: PageNumber, PageSize, SortBy, Search
}
```

### Response Contracts

```csharp
// Success response
var response = ApiResponse<PatientDto>.Ok(patientDto, "Patient retrieved", traceId);

// Error response
var error = ApiResponse.Failure(400, "Invalid patient data", new List<string> { "FirstName required" });

// Paginated response
var paginated = ApiResponse<PaginatedResponse<PatientDto>>.Ok(
    new PaginatedResponse<PatientDto>(patients, totalCount, pageNum, pageSize)
);

// Health check
var health = new HealthCheckResponse("Healthy", "PatientService", "1.0");
health.AddComponent("Database", "Healthy");
health.AddComponent("Cache", "Degraded", "Slow response times");
```

## Design Principles

✅ **Single Responsibility** - Each class has one purpose  
✅ **Clear Contracts** - Request/response types are explicit  
✅ **Consistent Envelopes** - All responses use ApiResponse  
✅ **Pagination Support** - Built-in for list operations  
✅ **Audit Trail** - DTOs include creation/modification info  
✅ **Healthcare Domain** - DTOs organized by domain (Healthcare/)

