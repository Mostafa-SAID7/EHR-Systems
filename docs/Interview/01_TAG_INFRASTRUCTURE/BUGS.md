# Tag Infrastructure - Known Bugs & Issues

## Active Bugs

### Bug #1: Tag Caching Race Condition
**Severity**: Medium  
**Status**: In Progress  
**Reported**: [Date]  

#### Description
When multiple services apply tags simultaneously to the same resource, Redis cache becomes stale.

#### Root Cause
- Tag cache key doesn't include version number
- No cache invalidation on concurrent updates
- Race condition in ITagQueryService.GetResourceTagsAsync

#### Steps to Reproduce
```
1. Open Patient with tags in two browser tabs
2. Add same tag in both tabs simultaneously
3. Refresh - one tag is missing from cache
```

#### Impact
- Users see inconsistent tag state
- Partial tag application
- Requires page refresh to see correct state

#### Fix Priority
**High** - Affects core feature reliability

#### Proposed Fix
```csharp
// Add version-based cache key
private string GetCacheKey(Guid resourceId, string resourceType)
{
    var version = _cacheService.Get<int>($"tag-version:{resourceId}");
    return $"tags:{resourceType}:{resourceId}:v{version}";
}

// Increment version on any modification
private void InvalidateCache(Guid resourceId)
{
    _cacheService.IncrementVersion($"tag-version:{resourceId}");
}
```

---

### Bug #2: Soft Delete Query Filter Not Applied in All Contexts
**Severity**: Medium  
**Status**: Investigating  

#### Description
Some queries return archived tags when they shouldn't.

#### Root Cause
- Global query filter only applied to DbContext.Tags
- Direct SQL queries bypass EF Core filters
- TagQueryService has custom queries without filter

#### Current Behavior
```csharp
// ❌ Returns archived tags
var allTags = await _context.Tags.ToListAsync();

// ✓ Excludes archived (has filter)
var activeTags = await _tagService.GetActiveTagsAsync();

// ❌ Returns archived via raw SQL
var tags = await _context.Tags
    .FromSqlRaw("SELECT * FROM Tags WHERE Category = @p0", category)
    .ToListAsync();
```

#### Impact
- Archived tags appear in dropdowns
- Data consistency issues
- Confusing UX

#### Proposed Fix
- Always use EF Core queries instead of raw SQL
- Create helper method that ensures filter applied
- Add tests to verify filter coverage

---

### Bug #3: AllowedServices String Parsing Error
**Severity**: Low  
**Status**: Resolved  

#### Description
Tag.AllowedServices = "Patient, Appointment" fails to validate correctly due to whitespace.

#### Root Cause
String comparison doesn't trim whitespace before split.

#### Fixed In
Version 1.2.1

#### Fix Applied
```csharp
public bool IsAllowedForService(string serviceName)
{
    if (string.IsNullOrEmpty(AllowedServices))
        return true;
    
    return AllowedServices
        .Split(',')
        .Select(s => s.Trim())  // ← Added trim
        .Contains(serviceName, StringComparer.OrdinalIgnoreCase);
}
```

---

## Bug Tracking Template

For new bugs, use this format:

### Bug #N: [Title]
**Severity**: Critical | High | Medium | Low  
**Status**: New | Investigating | In Progress | Blocked | Resolved  
**Reported By**: [Name/Date]  

#### Description
[What is the bug?]

#### Root Cause
[Why does it happen?]

#### Steps to Reproduce
[How to see it?]

#### Impact
[What breaks?]

#### Proposed Fix
[How to fix it?]

#### Code Example
```csharp
// Optional: Show code involved
```

---

## Performance Issues

### Issue: Tag Query N+1 Problem
**Impact**: Slow tag loading when fetching multiple resources  
**Fix Applied**: Batch query optimization in v1.3  

### Issue: Memory Leak in TagQueryCache
**Impact**: Memory usage grows over time  
**Status**: Monitoring  
**Workaround**: Cache TTL set to 1 hour  

---

## Regression Risks

### Risk: Changing ApplyTagAsync Signature
- **Impact**: All service implementations must update
- **Mitigation**: Use overload, mark old method deprecated

### Risk: Removing Soft Delete Support
- **Impact**: Compliance violations
- **Mitigation**: Keep IsArchived column permanently

---

## Test Coverage Gaps

| Area | Coverage | Issue |
|------|----------|-------|
| Concurrent tag operations | 0% | Race conditions not tested |
| Cache invalidation | 60% | Edge cases missing |
| Service restrictions | 85% | Good coverage |
| Audit trail | 95% | Comprehensive |

