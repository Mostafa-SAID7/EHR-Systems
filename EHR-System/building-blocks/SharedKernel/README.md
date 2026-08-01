# SharedKernel Package

Core DDD building blocks for all EHR microservices.

## Folder Structure

Each class has single responsibility, organized by concern:

### `Domain/` - Domain-Driven Design Foundations

#### Entities
- **BaseEntity.cs** - Core entity with ID, soft delete, correlation ID
- **AuditableEntity.cs** - Extends BaseEntity with audit trail (CreatedAt, UpdatedAt, etc.)
- **IEntity.cs** - Core entity contract (Id, IsDeleted)
- **IAuditableEntity.cs** - Auditable entity contract

#### ValueObjects
- **ValueObject.cs** - Base value object (value semantics, immutability)
- **ValueObjects/Address.cs** - Physical address value object
- **ValueObjects/EmailAddress.cs** - Email value object
- **ValueObjects/PhoneNumber.cs** - Phone number value object

#### Specifications
- **Specifications/Specification.cs** - DDD specification pattern (filtering, paging, eager loading)

#### Repositories (Abstractions)
- **Repositories/IRepository.cs** - Generic CRUD interface
- **Repositories/ISpecificationRepository.cs** - Specification-based queries
- **Repositories/IUnitOfWork.cs** - Transaction coordination

#### Domain Events
- **Events/IDomainEvent.cs** - Domain event contract
- **Events/IAggregateRoot.cs** - Aggregate root interface

#### Business Rules
- **Rules/IBusinessRule.cs** - Business rule pattern
- **Rules/BusinessRuleException.cs** - Exception when rule violated

### `Result/` - Railway-Oriented Programming

Functional error handling (instead of exceptions):

- **Result.cs** - Basic Result (success/failure without value)
- **ResultT.cs** - Result<T> (success with value or failure)
- **ResultExtensions.cs** - Functional combinators (Map, FlatMap, Match, Bind, Recover, Fold)

### `Guards/` - Input Validation

- **Guard.cs** - Guard clauses for parameter validation (AgainstNull, AgainstNullOrEmpty, etc.)

---

## Usage Examples

### Entity Hierarchy

```csharp
// Use BaseEntity for entities that don't need audit trail
public class Product : BaseEntity
{
    public string Name { get; set; }
}

// Use AuditableEntity for entities that need full audit trail
public class Patient : AuditableEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

### Result Pattern

```csharp
// Creating results
var success = Result<int>.Success(42);
var failure = Result<int>.Failure("Invalid input");

// Using results
success
    .Map(x => x * 2)
    .Match(
        onSuccess: value => $"Result: {value}",
        onFailure: error => $"Error: {error}"
    );

// Chaining operations
Result<User>.Success(user)
    .Bind(u => ValidateUser(u))
    .FlatMap(u => SaveUser(u))
    .Recover(error => CreateDefaultUser());
```

### Guard Clauses

```csharp
public void CreateUser(string email, int age)
{
    Guard.AgainstNullOrEmpty(email, nameof(email));
    Guard.AgainstNegative(age, nameof(age));
    Guard.AgainstOutOfRange(age, 18, 120, nameof(age));
    
    // Continue with validated inputs
}
```

### Specification Pattern

```csharp
public class GetActivePatientsByNameSpec : Specification<Patient>
{
    public GetActivePatientsByNameSpec(string name)
    {
        Criteria = p => !p.IsDeleted && p.FirstName.Contains(name);
        AddInclude(p => p.Appointments);
        OrderBy = p => p.LastName;
        ApplyPaging(skip: 0, take: 20);
    }
}

// Usage
var spec = new GetActivePatientsByNameSpec("John");
var patients = await _repository.GetAsync(spec);
```

### Value Objects

```csharp
var email = EmailAddress.Create("john@example.com");
if (email.IsSuccess)
{
    var localPart = email.Value.GetLocalPart();  // "john"
}

var address = Address.Create(
    street: "123 Main St",
    city: "Springfield",
    state: "IL",
    postalCode: "62701",
    country: "USA"
);
```

---

## Single Responsibility Principle

Each file has **one responsibility**:

| File | Responsibility |
|------|-----------------|
| BaseEntity.cs | Core entity (ID, soft delete) |
| AuditableEntity.cs | Audit trail (CreatedAt, UpdatedAt) |
| ValueObject.cs | Value semantics and equality |
| Result.cs | Success result without value |
| ResultT.cs | Success result with value |
| ResultExtensions.cs | Functional combinators |
| Guard.cs | Input validation |
| Specification.cs | DDD specification pattern |
| IBusinessRule.cs | Business rule contract |
| BusinessRuleException.cs | Rule violation exception |

---

## Architecture Benefits

✅ **Clear Separation of Concerns** - Each class does one thing well  
✅ **Easy to Test** - Small, focused classes  
✅ **Reusable** - Used across all 12 microservices  
✅ **Type-Safe** - Strong abstractions  
✅ **Extensible** - Open for extension (inheritance), closed for modification  

