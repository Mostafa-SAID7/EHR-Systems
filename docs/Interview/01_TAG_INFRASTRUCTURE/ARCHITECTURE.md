# Tag Infrastructure - Architecture & Design

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client (Web UI)                           │
└──────────────────────┬──────────────────────────────────────────┘
                       │
          ┌────────────┼────────────┐
          │            │            │
      ┌───▼──┐    ┌───▼──┐    ┌───▼──┐
      │Patient│    │Appt  │    │Bill  │
      │Service│    │Service│   │Service
      └───┬──┘    └───┬──┘    └───┬──┘
          │            │            │
          └────────────┼────────────┘
                       │
          ┌────────────▼────────────┐
          │  ITagService Interface  │
          │  (Centralized Logic)    │
          └────────────┬────────────┘
                       │
          ┌────────────┴────────────┐
          │                         │
      ┌───▼──────┐          ┌──────▼───┐
      │TagService│          │Category  │
      │(Common)  │          │Provider  │
      └───┬──────┘          └──────┬───┘
          │                        │
    ┌─────┴────────┬───────────────┴─────┐
    │              │                     │
┌───▼──┐      ┌────▼─────┐        ┌─────▼──┐
│ Tags │      │Tag        │        │Audit   │
│Table │      │Associations       │Trail   │
└──────┘      └──────────┘        └────────┘
    │              │                     │
    └──────────────┴─────────────────────┘
                   │
            ┌──────▼──────┐
            │  Database   │
            │  (SQL)      │
            └─────────────┘
```

---

## Core Components

### 1. ITagService Interface
**Location**: `EHRPlatform.Common/Services/ITagService.cs`

```csharp
public interface ITagService
{
    // Apply/Remove Operations
    Task<TagAssociation> ApplyTagAsync(Guid resourceId, string resourceType, 
        Guid tagId, string serviceName, string? appliedBy = null);
    
    Task<TagAssociation[]> ApplyTagsInBatchAsync(Guid resourceId, string resourceType, 
        Guid[] tagIds, string serviceName, string? appliedBy = null);
    
    Task<bool> RemoveTagAsync(Guid resourceId, string resourceType, 
        Guid tagId, string serviceName);
    
    // Query Operations
    Task<Tag?> GetTagByIdAsync(Guid tagId);
    
    // Validation
    Task<bool> ValidateTagAsync(Guid tagId, string serviceName);
}
```

**Responsibility**: Define contract for all tag operations  
**Usage**: Injected into controllers and handlers

---

### 2. TagService Implementation
**Location**: `EHRPlatform.Common/Services/TagService.cs`

```csharp
public class TagService : ITagService
{
    private readonly ITagDbContext _context;
    private readonly ITagQueryService _queryService;
    private readonly IDistributedCache _cache;

    public TagService(ITagDbContext context, 
        ITagQueryService queryService,
        IDistributedCache cache)
    {
        _context = context;
        _queryService = queryService;
        _cache = cache;
    }

    public async Task<TagAssociation> ApplyTagAsync(/* ... */)
    {
        // 1. Validation
        // 2. Create association
        // 3. Save to database
        // 4. Invalidate cache
        // 5. Return result
    }
}
```

**Responsibility**: Core business logic for tag operations  
**Key Methods**: ApplyTagAsync, RemoveTagAsync, ValidateTagAsync  
**Dependencies**: ITagDbContext, ITagQueryService, IDistributedCache

---

### 3. ITagQueryService
**Location**: `EHRPlatform.Common/Services/ITagQueryService.cs`

```csharp
public interface ITagQueryService
{
    Task<IEnumerable<Tag>> GetResourceTagsAsync(Guid resourceId, 
        string resourceType);
    
    Task<IEnumerable<Tag>> GetTagsByCategoryAsync(string category);
    
    Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm);
    
    Task<bool> TagExistsAsync(Guid tagId, Guid resourceId, 
        string resourceType);
}
```

**Responsibility**: Query operations only (CQRS Query side)  
**Cache Strategy**: 1-hour TTL with smart invalidation  
**Performance**: Optimized queries with indexes

---

### 4. ICategoryProvider Pattern
**Location**: `EHRPlatform.Common/Categories/ICategoryProvider.cs`

```csharp
public interface ICategoryProvider
{
    string ServiceName { get; }
    Task<IEnumerable<TagCategory>> GetCategoriesAsync();
}
```

**Purpose**: Service-specific tag categories  
**Implementations**:
- `PatientCategoryProvider`
- `AppointmentCategoryProvider`
- `BillingCategoryProvider`

**Example**:
```csharp
public class PatientCategoryProvider : ICategoryProvider
{
    public string ServiceName => "Patient";
    
    public async Task<IEnumerable<TagCategory>> GetCategoriesAsync()
    {
        return new[]
        {
            new TagCategory { Name = "Priority", 
                Options = new[] { "VIP", "Standard", "Low" } },
            new TagCategory { Name = "Health", 
                Options = new[] { "Chronic", "Acute", "Recovery" } }
        };
    }
}
```

---

### 5. Entities

#### Tag Entity
```csharp
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string? AllowedServices { get; set; }  // Comma-separated
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    
    // Soft Delete
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    // Analytics
    public int UsageCount { get; set; }
    
    // Relations
    public ICollection<TagAssociation> Associations { get; set; }
}
```

#### TagAssociation Entity
```csharp
public class TagAssociation
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public string ResourceType { get; set; }  // "Patient", "Appointment", etc.
    public Guid TagId { get; set; }
    public string ServiceName { get; set; }
    
    // Audit
    public DateTime AppliedAt { get; set; }
    public string? AppliedBy { get; set; }
    public DateTime? RemovedAt { get; set; }
    public string? RemovedBy { get; set; }
    
    // Soft Delete (history)
    public bool IsArchived { get; set; }
    
    // Relations
    public Tag? Tag { get; set; }
}
```

---

## CQRS Pattern Implementation

### Command Side (Write Operations)

```csharp
// Commands
public record ApplyTagCommand(Guid ResourceId, string ResourceType, 
    Guid TagId, string ServiceName, string? AppliedBy) 
    : IRequest<TagAssociation>;

public record RemoveTagCommand(Guid ResourceId, string ResourceType, 
    Guid TagId, string ServiceName) 
    : IRequest<bool>;

// Handlers
public class ApplyTagCommandHandler : IRequestHandler<ApplyTagCommand, TagAssociation>
{
    private readonly ITagService _tagService;
    
    public async Task<TagAssociation> Handle(ApplyTagCommand request, 
        CancellationToken cancellationToken)
    {
        return await _tagService.ApplyTagAsync(
            request.ResourceId,
            request.ResourceType,
            request.TagId,
            request.ServiceName,
            request.AppliedBy
        );
    }
}
```

**Responsibility**: Modify tag state  
**Pattern**: Mediator pattern with MediatR  
**Validation**: Performed before command execution

---

### Query Side (Read Operations)

```csharp
// Queries
public record GetResourceTagsQuery(Guid ResourceId, string ResourceType) 
    : IRequest<IEnumerable<Tag>>;

// Handlers
public class GetResourceTagsQueryHandler 
    : IRequestHandler<GetResourceTagsQuery, IEnumerable<Tag>>
{
    private readonly ITagQueryService _queryService;
    
    public async Task<IEnumerable<Tag>> Handle(GetResourceTagsQuery request, 
        CancellationToken cancellationToken)
    {
        return await _queryService.GetResourceTagsAsync(
            request.ResourceId,
            request.ResourceType
        );
    }
}
```

**Responsibility**: Query tag data without side effects  
**Pattern**: Optimized for read performance  
**Caching**: Automatic via Redis

---

## Data Flow Diagram

### Applying a Tag

```
1. POST /api/patients/{id}/tags
   ├─ Controller receives request
   ├─ Validates input (model binding)
   │
2. CreateMediator.Send(ApplyTagCommand)
   ├─ Validation pipeline
   ├─ Authorization checks
   │
3. ApplyTagCommandHandler.Handle
   ├─ Call ITagService.ApplyTagAsync
   │
4. TagService.ApplyTagAsync
   ├─ Check tag exists
   ├─ Check service allowed
   ├─ Create TagAssociation
   ├─ Save to database
   ├─ Invalidate cache
   │
5. Return TagAssociation to controller
   │
6. HTTP 201 Created + JSON response
```

### Querying Tags

```
1. GET /api/patients/{id}/tags
   ├─ Controller receives request
   │
2. Mediator.Send(GetResourceTagsQuery)
   │
3. GetResourceTagsQueryHandler.Handle
   ├─ Call ITagQueryService.GetResourceTagsAsync
   │
4. TagQueryService.GetResourceTagsAsync
   ├─ Check Redis cache
   ├─ If hit: Return cached tags
   ├─ If miss:
   │  ├─ Query database
   │  ├─ Cache result (1 hour TTL)
   │  └─ Return tags
   │
5. Return tags to controller
   │
6. HTTP 200 OK + JSON array
```

---

## Dependency Injection Setup

```csharp
public static void AddTagServices(this IServiceCollection services)
{
    // Core services
    services.AddScoped<ITagService, TagService>();
    services.AddScoped<ITagQueryService, TagQueryService>();
    
    // Category providers
    services.AddScoped<ICategoryProvider, PatientCategoryProvider>();
    services.AddScoped<ICategoryProvider, AppointmentCategoryProvider>();
    services.AddScoped<ICategoryProvider, BillingCategoryProvider>();
    
    // DbContext
    services.AddScoped<ITagDbContext>(sp => 
        sp.GetRequiredService<EHRPlatformDbContext>());
    
    // Cache
    services.AddStackExchangeRedisCache(options =>
        options.Configuration = "localhost:6379"
    );
}
```

**Location**: `EHRPlatform.Common/Extensions/ServiceCollectionExtensions.cs`  
**Usage**: `services.AddTagServices()` in Program.cs

---

## Database Schema

### Tags Table
```sql
CREATE TABLE Tags (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    AllowedServices NVARCHAR(200),
    
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100),
    ModifiedAt DATETIME2,
    ModifiedBy NVARCHAR(100),
    
    IsArchived BIT NOT NULL DEFAULT 0,
    ArchivedAt DATETIME2,
    
    UsageCount INT NOT NULL DEFAULT 0,
    
    UNIQUE (Name, Category),
    INDEX IX_Category ON Category,
    INDEX IX_IsArchived ON IsArchived
);
```

### TagAssociations Table
```sql
CREATE TABLE TagAssociations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ResourceId UNIQUEIDENTIFIER NOT NULL,
    ResourceType NVARCHAR(50) NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,
    ServiceName NVARCHAR(50) NOT NULL,
    
    AppliedAt DATETIME2 NOT NULL,
    AppliedBy NVARCHAR(100),
    RemovedAt DATETIME2,
    RemovedBy NVARCHAR(100),
    
    IsArchived BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (TagId) REFERENCES Tags(Id),
    COMPOSITE INDEX (TagId, ResourceId, ResourceType),
    INDEX IX_IsArchived ON IsArchived
);
```

---

## Error Handling

```csharp
public class TagNotFoundException : Exception
{
    public TagNotFoundException(Guid tagId) 
        : base($"Tag '{tagId}' not found") { }
}

public class ServiceRestrictionException : Exception
{
    public ServiceRestrictionException(string tagName, string serviceName)
        : base($"Tag '{tagName}' not allowed for service '{serviceName}'") { }
}

public class DuplicateTagAssociationException : Exception
{
    public DuplicateTagAssociationException(Guid resourceId, Guid tagId)
        : base($"Tag '{tagId}' already associated with resource '{resourceId}'") { }
}
```

---

## Extension Points

### Adding a New Service's Tags
1. Create `{Service}CTagProvider : ICategoryProvider`
2. Register in DI
3. Create `{Service}TagsController`
4. Done!

### Adding New Tag Features
1. Extend `ITagService` interface
2. Implement in `TagService`
3. Implement in handlers/controllers
4. Update tests

