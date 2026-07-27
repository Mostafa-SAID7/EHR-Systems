# Tag Endpoints Testing Guide

## Overview

This guide covers testing strategies for the new tag endpoints implemented across Patient, Appointment, and Billing services.

**Build Status:** ✅ 0 errors, 58 NuGet warnings

---

## Test Structure

### Test Framework
- **Framework:** xUnit 2.6.2
- **Mocking:** Moq
- **Logger:** ILogger<T> mock
- **Test Runner:** xunit.runner.visualstudio

### Test Project
```
backend/tests/EHRPlatform.Tests.Unit/
├── Common/
│   ├── Tags/
│   │   ├── TagServiceTests.cs
│   │   └── TagAssignmentCommandHandlerTests.cs
├── Services/
│   ├── Patient/
│   │   └── Controllers/
│   │       └── PatientTagsControllerTests.cs
│   ├── Appointment/
│   │   └── Controllers/
│   │       └── AppointmentTagsControllerTests.cs
│   └── Billing/
│       └── Controllers/
│           └── InvoiceTagsControllerTests.cs
```

---

## Unit Test Patterns

### 1. Tag Service Tests (`TagServiceTests.cs`)

#### Create Tag
```csharp
[Fact]
public async Task CreateTagAsync_WithValidData_CreatesTag()
{
    // Arrange - Setup mocks and test data
    var tagName = "High Priority";
    var category = "Priority";
    var serviceName = "Patient";
    
    _mockTagRepository
        .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    
    // Act - Execute the method
    var result = await _tagService.CreateTagAsync(tagName, category, serviceName, "Description", CancellationToken.None);
    
    // Assert - Verify results and mock calls
    Assert.NotNull(result);
    Assert.Equal(tagName, result.Name);
    _mockTagRepository.Verify(x => x.AddAsync(...), Times.Once);
}
```

**What to test:**
- Tag creation with all fields
- Tag name validation
- Category assignment
- Service scope verification

#### Apply Tag
```csharp
[Fact]
public async Task ApplyTagAsync_WithValidData_CreatesAssociation()
{
    // Setup mocks for tag lookup and association creation
    _mockTagRepository
        .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(tag);
    
    // Execute
    var result = await _tagService.ApplyTagAsync(resourceId, resourceType, tagId, serviceName, null, "system", CancellationToken.None);
    
    // Verify association created
    Assert.Equal(resourceId, result.ResourceId);
    Assert.Equal(tagId, result.TagId);
}
```

**What to test:**
- Tag application to resources
- Resource type validation
- Service authorization check
- Association creation success

#### Remove Tag
```csharp
[Fact]
public async Task RemoveTagAsync_WithExistingAssociation_DeletesAssociation()
{
    // Setup - Mock association lookup and delete
    _mockAssociationRepository
        .Setup(x => x.FirstOrDefaultAsync(..., It.IsAny<CancellationToken>()))
        .ReturnsAsync(association);
    
    // Execute
    var result = await _tagService.RemoveTagAsync(resourceId, resourceType, tagId, CancellationToken.None);
    
    // Verify - Association removed
    Assert.True(result);
    _mockAssociationRepository.Verify(x => x.DeleteAsync(association, ...), Times.Once);
}
```

**What to test:**
- Successful tag removal
- Non-existent tag handling
- Association deletion

### 2. Command Handler Tests (`TagAssignmentCommandHandlerTests.cs`)

#### Apply Tags Command
```csharp
[Fact]
public async Task ApplyTagsCommandHandler_WithValidCommand_AppliesTagsSuccessfully()
{
    var command = new ApplyTagsCommand
    {
        ResourceId = resourceId,
        ResourceType = "Patient",
        TagIds = new[] { tagId1, tagId2 },
        ServiceName = "Patient",
        AppliedBy = "user123"
    };
    
    // Mock tag service to return valid tags
    _mockTagService
        .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(validTag);
    
    var result = await _handler.Handle(command, CancellationToken.None);
    
    Assert.True(result.Success);
    Assert.Contains("Successfully applied", result.Message);
}
```

**Test Cases:**
- ✅ Valid tags applied successfully
- ✅ Non-existent tags handled gracefully (partial success)
- ✅ Unauthorized service blocked
- ✅ Empty tag list rejected
- ✅ Logging captured correctly

#### Remove Tag Command
```csharp
[Fact]
public async Task RemoveTagCommandHandler_WithExistingTag_RemovesSuccessfully()
{
    var command = new RemoveTagCommand
    {
        ResourceId = resourceId,
        ResourceType = "Patient",
        TagId = tagId,
        ServiceName = "Patient"
    };
    
    _mockTagService
        .Setup(x => x.RemoveTagAsync(...))
        .ReturnsAsync(true);
    
    var result = await _handler.Handle(command, CancellationToken.None);
    
    Assert.True(result.Success);
}
```

**Test Cases:**
- ✅ Existing tag removed
- ✅ Non-existent tag returns appropriate error
- ✅ Service authorization validated

### 3. Controller Tests (`PatientTagsControllerTests.cs`)

#### GET /api/v1/patients/{patientId}/tags
```csharp
[Fact]
public async Task GetPatientTags_WithExistingTags_ReturnsOkWithTags()
{
    var patientId = Guid.NewGuid();
    var tags = new List<TagDto>
    {
        new TagDto { Id = Guid.NewGuid(), Name = "VIP", Category = "Priority" },
        new TagDto { Id = Guid.NewGuid(), Name = "High Risk", Category = "Health" }
    };
    
    _mockTagQueryService
        .Setup(x => x.GetResourceTagsAsync(patientId, nameof(PatientEntity), It.IsAny<CancellationToken>()))
        .ReturnsAsync(tags);
    
    var result = await _controller.GetPatientTags(patientId, CancellationToken.None);
    
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.Equal(200, okResult.StatusCode);
}
```

**Test Cases:**
- ✅ Returns 200 OK with tags
- ✅ Returns empty array when no tags
- ✅ Returns 500 on service error
- ✅ Correct resource type passed

#### POST /api/v1/patients/{patientId}/tags
```csharp
[Fact]
public async Task ApplyPatientTags_WithValidCommand_ReturnsOkWithResponse()
{
    var command = new ApplyTagsCommand { TagIds = new[] { Guid.NewGuid() } };
    var response = new TagAssignmentResponse { Success = true, Message = "Successfully applied 1 tag(s)" };
    
    _mockMediator
        .Setup(x => x.Send(It.IsAny<ApplyTagsCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);
    
    var result = await _controller.ApplyPatientTags(patientId, command, CancellationToken.None);
    
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.True((okResult.Value as TagAssignmentResponse)?.Success);
}
```

**Test Cases:**
- ✅ Returns 200 OK on success
- ✅ Returns 400 BadRequest on validation error
- ✅ Resource type set correctly
- ✅ Service name set to "Patient"

#### DELETE /api/v1/patients/{patientId}/tags/{tagId}
```csharp
[Fact]
public async Task RemovePatientTag_WithExistingTag_ReturnsNoContent()
{
    _mockMediator
        .Setup(x => x.Send(It.IsAny<RemoveTagCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new TagAssignmentResponse { Success = true });
    
    var result = await _controller.RemovePatientTag(patientId, tagId, CancellationToken.None);
    
    Assert.IsType<NoContentResult>(result); // 204 No Content
}
```

**Test Cases:**
- ✅ Returns 204 No Content on success
- ✅ Returns 404 Not Found when tag not on resource
- ✅ Returns 500 on service error

#### PUT /api/v1/patients/{patientId}/tags
```csharp
[Fact]
public async Task SetPatientTags_WithValidCommand_ReturnsOkWithResponse()
{
    var tagIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
    var response = new TagAssignmentResponse
    {
        Success = true,
        Message = "Successfully set 2 tag(s)",
        TotalTagsOnResource = 2
    };
    
    _mockMediator
        .Setup(x => x.Send(It.IsAny<SetResourceTagsCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);
    
    var result = await _controller.SetPatientTags(patientId, command, CancellationToken.None);
    
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.Equal(2, (okResult.Value as TagAssignmentResponse)?.TotalTagsOnResource);
}
```

**Test Cases:**
- ✅ Returns 200 OK with updated tags
- ✅ Returns 400 BadRequest on validation error
- ✅ All tags replaced (no merge)

---

## Integration Test Patterns

### End-to-End Tag Flow
```csharp
[Fact]
public async Task TagWorkflow_CreateApplyQueryRemove_CompletesSuccessfully()
{
    // 1. Create tag
    var tagId = await _tagService.CreateTagAsync("VIP", "Priority", "Patient", "VIP Patients", CancellationToken.None);
    
    // 2. Apply to resource
    var applied = await _tagService.ApplyTagAsync(patientId, "Patient", tagId.Id, "Patient", null, "system", CancellationToken.None);
    Assert.NotNull(applied);
    
    // 3. Query tags
    var tags = await _tagQueryService.GetResourceTagsAsync(patientId, "Patient", CancellationToken.None);
    Assert.Contains(tags, t => t.Id == tagId.Id);
    
    // 4. Remove tag
    var removed = await _tagService.RemoveTagAsync(patientId, "Patient", tagId.Id, CancellationToken.None);
    Assert.True(removed);
}
```

---

## Test Data Fixtures

### Common Test Data
```csharp
public class TagTestFixtures
{
    public static readonly Guid PatientId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid AppointmentId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid InvoiceId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    
    public static Tag CreateTag(string name = "Test", string category = "Test", string service = "Patient")
    {
        return new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Category = category,
            ServiceName = service,
            Description = $"Test {name} tag",
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static TagAssociation CreateAssociation(Guid resourceId, Guid tagId, string resourceType = "Patient")
    {
        return new TagAssociation
        {
            Id = Guid.NewGuid(),
            ResourceId = resourceId,
            ResourceType = resourceType,
            TagId = tagId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

## Running Tests

### Run All Tests
```bash
cd backend
dotnet test EHRPlatform.sln -c Release
```

### Run Specific Test Class
```bash
dotnet test --filter "ClassName=PatientTagsControllerTests"
```

### Run With Coverage
```bash
dotnet test EHRPlatform.sln --collect:"XPlat Code Coverage"
```

### Run Specific Test Method
```bash
dotnet test --filter "Name=GetPatientTags_WithExistingTags_ReturnsOkWithTags"
```

---

## Test Coverage Goals

| Component | Target Coverage | Current |
|-----------|-----------------|---------|
| TagService | 90% | TBD |
| ApplyTagsCommandHandler | 95% | TBD |
| RemoveTagCommandHandler | 95% | TBD |
| PatientTagsController | 85% | TBD |
| AppointmentTagsController | 85% | TBD |
| InvoiceTagsController | 85% | TBD |

---

## Mock Setup Best Practices

### 1. Tag Service Mocks
```csharp
private void SetupTagServiceMocks(Tag tag = null)
{
    _mockTagRepository
        .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(tag ?? CreateDefaultTag());
    
    _mockUnitOfWork
        .Setup(x => x.Repository<Tag>())
        .Returns(_mockTagRepository.Object);
}
```

### 2. Mediator Mocks
```csharp
private void SetupMediatorMocks(TagAssignmentResponse response = null)
{
    _mockMediator
        .Setup(x => x.Send(
            It.IsAny<ApplyTagsCommand>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(response ?? CreateSuccessResponse());
}
```

### 3. Logger Verification
```csharp
private void VerifyLogging(LogLevel level, string message)
{
    _mockLogger.Verify(
        x => x.Log(
            level,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

---

## Common Assertions

```csharp
// Controller responses
Assert.IsType<OkObjectResult>(result);
Assert.IsType<BadRequestObjectResult>(result);
Assert.IsType<NotFoundObjectResult>(result);
Assert.IsType<NoContentResult>(result);

// Response content
var okResult = result as OkObjectResult;
Assert.Equal(200, okResult.StatusCode);

// Mock calls
_mockService.Verify(x => x.Method(...), Times.Once);
_mockService.Verify(x => x.Method(...), Times.Never);
```

---

## Error Scenarios to Test

- ✅ Invalid tag IDs
- ✅ Non-existent resources
- ✅ Unauthorized service access
- ✅ Duplicate tag applications
- ✅ Concurrent tag operations
- ✅ Invalid resource types
- ✅ Null/empty collections
- ✅ Service exceptions
- ✅ Database transaction rollbacks

---

## Related Documentation

- [Tag Endpoints API Specification](../API/TAG_ENDPOINTS.md)
- [CQRS Pattern Implementation](./CQRS_IMPLEMENTATION.md)
- [Controller Organization](../Architecture/CONTROLLER_ORGANIZATION.md)
