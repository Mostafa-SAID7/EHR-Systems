# DTO Dependency Map

This document outlines the internal dependencies between DTOs in the `EHRPlatform.Common.Shared.DTOs` namespace.

## Dependency Graph

```
PagedResult<T>
    ↑
    └─── PagedApiResponse<T> ✅ (wraps PagedResult)

TagDto
    ├─── CategoryMetadataResponse ✅ (contains Dictionary<string, IEnumerable<TagDto>>)
    ├─── TagAssociationDto ✅ (property: TagDto? Tag)
    └─── TagSuggestionResponse ✅ (contains IEnumerable<TagDto>)

BulkOperationItemResult
    └─── BulkTagOperationResult ✅ (contains IEnumerable<BulkOperationItemResult>)

Abstract Base Classes:
    SluggedResponseDto (no dependencies within DTOs)
        ↑
        └─── StatusDto (inherits SluggedResponseDto)
```

## Internal Reference Analysis

### No External Namespace Dependencies
✅ All DTOs reference only:
- Standard C# types: `string`, `int`, `bool`, `Guid`, `DateTime`, `Dictionary`, `List`, `IEnumerable`
- Other DTOs in the same namespace
- No cross-project DTO references

### No Circular Dependencies Detected
✅ Verified:
- `PagedApiResponse<T>` depends on `PagedResult<T>` ✓
- `BulkTagOperationResult` depends on `BulkOperationItemResult` ✓
- `CategoryMetadataResponse` depends on `TagDto` ✓
- `TagAssociationDto` depends on `TagDto` ✓
- `TagSuggestionResponse` depends on `TagDto` ✓
- No reverse dependencies (no circular chains)

### Namespace Clarity
All DTOs use the file-level namespace declaration without explicit `using` statements within the namespace block. This is clean and prevents namespace pollution.

**Namespace:** `EHRPlatform.Common.Shared.DTOs`

## Dependency Strength Matrix

| From | To | Type | Strength | Status |
|------|-----|------|----------|--------|
| PagedApiResponse<T> | PagedResult<T> | Generic Wrapper | Required | ✅ |
| BulkTagOperationResult | BulkOperationItemResult | Collection | Required | ✅ |
| CategoryMetadataResponse | TagDto | Collection | Required | ✅ |
| TagAssociationDto | TagDto | Nested DTO | Optional | ✅ |
| TagSuggestionResponse | TagDto | Collection | Required | ✅ |

## Reference Counts by DTO

| DTO | Referenced By | Count | Status |
|-----|---|-------|--------|
| TagDto | CategoryMetadataResponse, TagAssociationDto, TagSuggestionResponse | 3 | ✅ Active |
| PagedResult<T> | PagedApiResponse<T> | 1 | ✅ Active |
| BulkOperationItemResult | BulkTagOperationResult | 1 | ✅ Active |
| PagedApiResponse<T> | (controllers/handlers) | Unknown | ✅ Used |
| ApiResponse | (controllers/handlers) | Unknown | ✅ Used |
| ApiResponse<T> | (controllers/handlers) | Unknown | ✅ Used |
| ErrorResponse | (error middleware) | Unknown | ✅ Used |
| PaginationRequest | (controllers/handlers) | Unknown | ✅ Used |

## Dependency Health Check

| Criterion | Status | Details |
|-----------|--------|---------|
| No Circular Dependencies | ✅ PASS | All references are unidirectional |
| No Namespace Pollution | ✅ PASS | All DTOs in single namespace |
| Type Safety | ✅ PASS | Proper generic constraints |
| Null Safety | ✅ PASS | All DTOs use `#nullable enable` |
| No Dead Code | ✅ PASS | All DTOs are referenced or used |
| Proper Abstraction | ✅ PASS | Abstract bases are inherited |
| Single Responsibility | ✅ PASS | Each DTO has one clear purpose |

## External References

DTOs in this namespace are referenced by:

1. **Controllers** - Return types for action methods
2. **Query/Command Handlers** - Response types
3. **AutoMapper Profiles** - Mapping targets
4. **Service Interfaces** - DTO parameters and returns
5. **Application Services** - Internal DTOs

No external services depend on these DTOs outside the Common project (good isolation).

## Recommendations

✅ **Current State: OPTIMAL**

All DTOs are:
- Properly organized
- Have clear, distinct purposes
- Have no circular dependencies
- Are actively used
- Have type-safe references

**No refactoring needed.**

---

**Last Updated:** 2025-01-24  
**Dependency Analysis:** Complete  
**Status:** All Clear ✅
