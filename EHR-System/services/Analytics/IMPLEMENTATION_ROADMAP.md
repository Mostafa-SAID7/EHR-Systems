# Analytics Service - Implementation Roadmap

**Start Date:** August 4, 2026  
**Target Completion:** Production Ready  
**Current Status:** 15% Complete (Critical Phase)

---

## PHASE 1: ARCHITECTURE FIXES (BLOCKING ISSUES)

**Duration:** 2-4 hours  
**Effort:** 4 tasks  
**Status:** 🔴 CRITICAL - Must complete before anything else

### Task 1.1: Fix Repository Interface Names
**Impact:** HIGH - Compilation will fail without this
**Files to Modify:**
- `Domain/Repositories/IMetricRepository.cs` → Change `Metric` to `AnalyticsMetric`
- `Domain/Repositories/IKPIRepository.cs` → Change `KPI` to `KPISummary`
- `Persistence/Repositories/AnalyticsMetricRepository.cs` → Update interface implementation
- `Persistence/Repositories/KPIRepository.cs` → Update interface implementation

**Changes:**
```csharp
// BEFORE: IMetricRepository
Task<Metric?> GetByIdAsync(Guid id);

// AFTER: IMetricRepository  
Task<AnalyticsMetric?> GetByIdAsync(Guid id);
```

### Task 1.2: Move Response DTOs to Contracts Layer
**Impact:** HIGH - Violates clean architecture
**Action:** Create 9 new response DTO files in `Contracts/Responses/`

**Files to Create:**
1. `CreateDashboardResponse.cs` (move from CreateDashboardCommand.cs)
2. `UpdateDashboardResponse.cs` (move from UpdateDashboardCommand.cs)
3. `DeleteDashboardResponse.cs` (move from DeleteDashboardCommand.cs)
4. `ExportDataResponse.cs` (move from ExportDataCommand.cs)
5. `GetKPISummaryResponse.cs` (move from GetKPISummaryQuery.cs)
6. `KPISummaryDto.cs` (move from GetKPISummaryQuery.cs)
7. `GetMetricsResponse.cs` (create new)
8. `MetricDataDto.cs` (move from GetMetricsQuery.cs)
9. `CreateReportResponse.cs` (create new)

**Files to Modify:**
- Update all using statements in Commands/Queries to import from Contracts.Responses

### Task 1.3: Extract Request DTOs to Contracts Layer
**Impact:** MEDIUM - Improves separation of concerns
**Action:** Create 6 new request DTO files in `Contracts/Requests/`

**Files to Create:**
1. `CreateDashboardRequestDto.cs` (extract from CreateDashboardCommand)
2. `CreateReportRequestDto.cs` (new)
3. `UpdateReportRequestDto.cs` (new)
4. `ExecuteReportRequestDto.cs` (new)
5. `CreateDashboardWidgetRequestDto.cs` (new)
6. `UpdateDashboardWidgetRequestDto.cs` (new)

**Files to Modify:**
- CreateDashboardCommand.cs (remove inline properties, use DTO)
- Controller (import from Contracts.Requests)

### Task 1.4: Fix Duplicate DependencyInjection
**Impact:** LOW - Code cleanup
**File:** `Contracts/DependencyInjection.cs`
**Action:** Delete lines 16-20 (duplicate method definition)

**Estimated Time:** 30 minutes

---

## PHASE 2: MISSING COMMANDS & QUERIES (HIGH PRIORITY)

**Duration:** 6-8 hours  
**Effort:** 12 files  
**Status:** 🟡 HIGH - Core feature gap

### Task 2.1: Dashboard Commands
**Files to Create:**
1. `CreateDashboardCommandHandler.cs` - CREATE operation
2. `GetDashboardsQuery.cs` & `GetDashboardsQueryHandler.cs` - LIST operation
3. `GetDashboardByIdQuery.cs` & `GetDashboardByIdQueryHandler.cs` - GET operation

**Implementation Pattern:**
```csharp
// GetDashboardsQueryHandler
public async Task<GetDashboardsResponse> Handle(GetDashboardsQuery request, ...)
{
    var dashboards = await _dashboardRepository.GetAllAsync(tenantId);
    return new GetDashboardsResponse { Dashboards = dashboards.ToList() };
}
```

### Task 2.2: Report Commands  
**Files to Create:**
1. `CreateReportCommand.cs` & `CreateReportCommandHandler.cs`
2. `UpdateReportCommand.cs` & `UpdateReportCommandHandler.cs`
3. `DeleteReportCommand.cs` & `DeleteReportCommandHandler.cs`
4. `ExecuteReportCommand.cs` & `ExecuteReportCommandHandler.cs`
5. `GetReportsQuery.cs` & `GetReportsQueryHandler.cs`
6. `GetReportExecutionsQuery.cs` & `GetReportExecutionsQueryHandler.cs`

### Task 2.3: Dashboard Widget Commands
**Files to Create:**
1. `CreateDashboardWidgetCommand.cs` & Handler
2. `UpdateDashboardWidgetCommand.cs` & Handler
3. `DeleteDashboardWidgetCommand.cs` & Handler
4. `GetDashboardWidgetsQuery.cs` & Handler

---

## PHASE 3: COMPLETE STUB HANDLERS (HIGH PRIORITY)

**Duration:** 4-6 hours  
**Effort:** 4 handlers  
**Status:** 🟡 HIGH - Currently broken

### Task 3.1: CreateDashboardCommandHandler
**Current State:** Command exists but no handler
**Implementation Steps:**
1. Create handler class
2. Inject IDashboardRepository
3. Validate input (name required)
4. Create dashboard entity
5. Save to repository
6. Publish DashboardCreatedEvent
7. Return response with dashboard ID

### Task 3.2: UpdateDashboardCommandHandler
**Current State:** Stub with TODOs
**Implementation Steps:**
1. Remove TODO comments
2. Inject IDashboardRepository
3. Validate dashboard exists
4. Update entity properties
5. Save to repository
6. Clear cache
7. Publish DashboardUpdatedEvent

### Task 3.3: DeleteDashboardCommandHandler
**Current State:** Stub with TODOs
**Implementation Steps:**
1. Remove TODO comments
2. Inject IDashboardRepository
3. Validate dashboard exists
4. Soft delete or hard delete
5. Clear cache
6. Publish DashboardDeletedEvent

### Task 3.4: GetMetricsQueryHandler
**Current State:** Command exists but handler is stub
**Implementation Steps:**
1. Inject IMetricRepository
2. Query by date range
3. Apply filters if provided
4. Implement pagination
5. Cache results (15 min TTL)
6. Return response with metadata

### Task 3.5: ExportDataCommandHandler
**Current State:** Stub with TODOs
**Implementation Steps:**
1. Inject IMetricRepository & IFileStorage service
2. Query data by date range
3. Convert to requested format (CSV/Excel/JSON/PDF)
4. Generate file bytes
5. Store in file storage service
6. Return download URL
7. Publish DataExportedEvent

---

## PHASE 4: ADD DOMAIN EVENTS (MEDIUM PRIORITY)

**Duration:** 2-3 hours  
**Effort:** 6 event files  
**Status:** 🟡 MEDIUM - Needed for event sourcing

### Task 4.1: Create Domain Events
**Files to Create in `Domain/Events/`:**

1. `DashboardCreatedEvent.cs`
2. `DashboardUpdatedEvent.cs`
3. `DashboardDeletedEvent.cs`
4. `ReportExecutedEvent.cs` (already exists as ReportExecutedEvent)
5. `DataExportedEvent.cs`
6. `MetricRecordedEvent.cs` (already exists but verify)

**Example:**
```csharp
public record DashboardCreatedEvent(Guid DashboardId, string Name, DateTime CreatedAt)
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Task 4.2: Publish Events from Handlers
**Add to each handler:**
```csharp
// In handler after saving:
await _messagePublisher.PublishAsync(new DashboardCreatedEvent(dashboard.Id, dashboard.Name, DateTime.UtcNow));
```

**Handlers to Update:**
- CreateDashboardCommandHandler
- UpdateDashboardCommandHandler
- DeleteDashboardCommandHandler
- ExportDataCommandHandler
- ExecuteReportCommandHandler

---

## PHASE 5: CACHING & OPTIMIZATION (LOW PRIORITY)

**Duration:** 2-3 hours  
**Effort:** 3 handlers  
**Status:** 🟢 LOW - Optimization

### Task 5.1: Implement Query Caching
**Handlers to Update:**
1. GetKPISummaryQueryHandler - Cache 15 minutes
2. GetMetricsQueryHandler - Cache 10 minutes
3. GetDashboardsQueryHandler - Cache 5 minutes

**Pattern:**
```csharp
var cacheKey = $"kpi_summary_{request.ForDate:yyyyMMdd}";
var cached = await _cacheService.GetAsync<GetKPISummaryResponse>(cacheKey);
if (cached != null) return cached;

// Query and cache...
await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
```

### Task 5.2: Implement Cache Invalidation
**Add to each update handler:**
```csharp
// Clear related caches
await _cacheService.RemoveAsync("dashboards_list");
await _cacheService.RemoveAsync($"dashboard_{command.DashboardId}");
```

---

## PHASE 6: FILE STORAGE INTEGRATION (LOW PRIORITY)

**Duration:** 2-3 hours  
**Effort:** 1 handler  
**Status:** 🟢 LOW - Export functionality

### Task 6.1: ExportDataCommandHandler Enhancement
**Add:**
1. Inject IFileStorageService (from building-blocks)
2. After generating file, call `_fileStorage.UploadAsync()`
3. Return download URL instead of file bytes
4. Add file metadata tracking

---

## PHASE 7: MULTI-TENANCY COMPLETION (LOW PRIORITY)

**Duration:** 1-2 hours  
**Effort:** 4 handlers  
**Status:** 🟢 LOW - Compliance

### Task 7.1: Add Tenant ID to All Queries
**Current:** Repository methods require tenantId but handlers don't pass it
**Action:** Inject ITenantContext in each handler and pass tenantId

**Pattern:**
```csharp
public class GetDashboardsQueryHandler : IRequestHandler<GetDashboardsQuery, GetDashboardsResponse>
{
    private readonly ITenantContext _tenantContext;
    
    public async Task<GetDashboardsResponse> Handle(...)
    {
        var tenantId = _tenantContext.TenantId;
        var dashboards = await _dashboardRepository.GetAllAsync(tenantId);
        ...
    }
}
```

---

## TESTING STRATEGY

### Unit Tests to Create
- CreateDashboardCommandHandler.Tests
- UpdateDashboardCommandHandler.Tests
- DeleteDashboardCommandHandler.Tests
- GetDashboardsQueryHandler.Tests
- GetKPISummaryQueryHandler.Tests

### Integration Tests to Create
- Dashboard API endpoint tests (POST, PUT, DELETE, GET)
- Metrics API endpoint tests
- Export functionality tests
- KPI summary tests

### Test Coverage Target: 70%+

---

## API ENDPOINTS - IMPLEMENTATION ORDER

### Priority 1: Dashboard CRUD (2 hours)
```
POST /api/v1/analytics/dashboards          [CreateDashboard]
GET /api/v1/analytics/dashboards           [GetDashboards]
GET /api/v1/analytics/dashboards/{id}      [GetDashboardById]
PUT /api/v1/analytics/dashboards/{id}      [UpdateDashboard]
DELETE /api/v1/analytics/dashboards/{id}   [DeleteDashboard]
```

### Priority 2: Metrics & Export (1.5 hours)
```
GET /api/v1/analytics/metrics              [GetMetrics] - Complete handler
POST /api/v1/analytics/export              [ExportData] - Complete handler
```

### Priority 3: Reports (3 hours)
```
POST /api/v1/analytics/reports             [CreateReport]
GET /api/v1/analytics/reports              [GetReports]
PUT /api/v1/analytics/reports/{id}         [UpdateReport]
DELETE /api/v1/analytics/reports/{id}      [DeleteReport]
POST /api/v1/analytics/reports/{id}/execute [ExecuteReport]
GET /api/v1/analytics/reports/{id}/executions [GetReportExecutions]
```

### Priority 4: Dashboard Widgets (2 hours)
```
POST /api/v1/analytics/dashboards/{id}/widgets           [CreateWidget]
PUT /api/v1/analytics/dashboards/{id}/widgets/{widgetId} [UpdateWidget]
DELETE /api/v1/analytics/dashboards/{id}/widgets/{widgetId} [DeleteWidget]
GET /api/v1/analytics/dashboards/{id}/widgets            [GetWidgets]
```

---

## TIMELINE ESTIMATE

| Phase | Duration | Status |
|-------|----------|--------|
| Phase 1: Architecture Fixes | 2-4h | 🔴 CRITICAL |
| Phase 2: Missing Commands/Queries | 6-8h | 🟡 HIGH |
| Phase 3: Complete Stub Handlers | 4-6h | 🟡 HIGH |
| Phase 4: Domain Events | 2-3h | 🟡 MEDIUM |
| Phase 5: Caching | 2-3h | 🟢 LOW |
| Phase 6: File Storage | 2-3h | 🟢 LOW |
| Phase 7: Multi-Tenancy | 1-2h | 🟢 LOW |
| **Testing & Verification** | **8-12h** | 🟡 MEDIUM |
| **TOTAL** | **~30-40 hours** | |

---

## SUCCESS CRITERIA

- [ ] All 7 API endpoints fully implemented (not stubs)
- [ ] 16+ handlers complete with no TODO comments
- [ ] All request/response DTOs in correct layers
- [ ] Repository interfaces compile without errors
- [ ] 70%+ unit test coverage
- [ ] All integration tests passing
- [ ] No duplicate class definitions
- [ ] Multi-tenancy working end-to-end
- [ ] Caching integrated and tested
- [ ] File export working
- [ ] Domain events publishing and being consumed

---

**Ready to Begin Phase 1?** YES / NO

**Execution Mode:** Systematic one-phase-at-a-time approach
**Review Frequency:** After each phase completion
