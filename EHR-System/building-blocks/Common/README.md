# Common Package

Cross-cutting abstractions shared across all services.

## Contents (51 files)

### Resilience (6 files)
- `ICircuitBreaker.cs` - Circuit breaker pattern (Open/Closed/Half-Open)
- `CircuitBreakerState.cs` - State enumeration
- `CircuitBreakerStats.cs` - Statistics tracking
- `IRetryPolicy.cs` - Retry logic abstraction
- `IBackoffStrategy.cs` - Backoff calculations
- `BackoffStrategyType.cs` - Strategy types (Exponential, Linear, Fixed)

### Background Jobs (4 files)
- `IBackgroundJobService.cs` - Queue and execute jobs
- `BackgroundJobStatus.cs` - Job status tracking
- `IJobScheduler.cs` - Cron/interval scheduling
- `JobScheduleConfig.cs` - Schedule configuration

### Search (11 files)
- `ISearchService.cs` - Execute full-text search
- `IIndexService.cs` - Manage indices
- `ISearchQueryBuilder.cs` - Fluent query building
- `SearchQuery.cs` - Query specification
- `SearchResult.cs` - Result data
- `SearchResultWithAggregations.cs` - Faceted results
- `AggregationBucket.cs` - Facet bucket
- `SearchHit.cs` - Result with highlighting
- `SearchFilter.cs` - Filter criteria
- `SortClause.cs` - Sort specification
- `IndexStats.cs` - Index statistics

### File Storage (8 files)
- `IFileStorage.cs` - Local/abstracted storage
- `FileMetadata.cs` - File information
- `ICloudStorage.cs` - Cloud provider interface
- `CloudFileReference.cs` - Cloud file reference
- `CloudFileProperties.cs` - Cloud file metadata
- `IFileValidationService.cs` - File validation
- `FileSecurityResult.cs` - Security scan result
- `ThreatLevel.cs` - Threat level enumeration

### Core Abstractions (7 files)
- `ISerializer.cs` - JSON serialization
- `ICacheService.cs` - Distributed caching
- `IIdGenerator.cs` - ID generation
- `IValidator.cs` - Generic validation
- `IMapper.cs` - Object mapping
- `IDateTimeProvider.cs` - Testable clock
- `IConfigurationProvider.cs` - Configuration access

### Enterprise (15 files)
- `IFeatureFlagService.cs` - Feature toggles
- `ISortingProvider.cs` - API sorting
- `IMiddleware.cs` - Pipeline middleware
- `IMiddlewarePipeline.cs` - Pipeline execution
- `Exception hierarchy` - Custom exceptions
- `Guard clauses` - Validation helpers
- `Extensions` - Utility extensions

---

## Usage

Import namespaces:
```csharp
using EHRPlatform.Common.Resilience;
using EHRPlatform.Common.Search;
using EHRPlatform.Common.FileStorage;
```

## Parent

[← Building Blocks](../README.md)
