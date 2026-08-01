# SharedKernel Package

Core domain and application patterns.

## Contents (36 files)

### CQRS (6 files)
- `ICommand.cs` - Command marker
- `ICommandHandler.cs` - Command handler
- `IQuery.cs` - Query marker
- `IQueryHandler.cs` - Query handler
- `CommandResult.cs` - Execution result
- `QueryResult.cs` - Query result

### Event Sourcing (4 files)
- `IEventStore.cs` - Event persistence
- `EventEnvelope.cs` - Event with metadata
- `ISnapshotStore.cs` - Snapshot storage
- `Snapshot.cs` - Snapshot data

### Repositories (4 files)
- `IRepository.cs` - Generic repository (20+ methods)
- `IUnitOfWork.cs` - Transaction coordination
- `RepositoryOptions.cs` - Query options
- `QuerySpecification.cs` - Query specification

### Domain (9 files)
- `IAggregateRoot.cs` - Aggregate root contract
- `BaseEntity.cs` - Base entity
- `AuditableEntity.cs` - Audit trail support
- `IEntity.cs` - Entity marker
- `IValueObject.cs` - Value object marker
- `ValueObject.cs` - Base value object
- `IAuditableEntity.cs` - Auditability contract
- `IDomainEvent.cs` - Domain event marker

### Specifications (7 files)
- `ISpecification.cs` - Specification contract
- `BaseSpecification.cs` - Base implementation
- `SpecificationBuilder.cs` - Fluent builder
- `IncludeExpression.cs` - Navigation includes
- `OrderByExpression.cs` - Sorting
- `PaginationExpression.cs` - Pagination
- `SearchExpression.cs` - Search criteria

### Result Pattern (3 files)
- `Result.cs` - Base result
- `ResultT.cs` - Generic result
- `ResultExtensions.cs` - Combinators (Map, FlatMap, Match)

### Services (3 files)
- `IApplicationService.cs` - Application service
- `IDomainService.cs` - Domain service
- `INotificationService.cs` - Notifications

---

## Usage

```csharp
using EHRPlatform.SharedKernel.CQRS;
using EHRPlatform.SharedKernel.EventSourcing;
using EHRPlatform.SharedKernel.Domain;
```

## Parent

[← Building Blocks](../README.md)
