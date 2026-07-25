# Angular RxJS Operators — Deep Guide

## 1. Essential Operators for HTTP & Forms

### switchMap — Cancel Previous, Use Latest (Search / HTTP)
```typescript
// Search: cancel in-flight request when user types again
this.searchControl.valueChanges.pipe(
  debounceTime(300),
  distinctUntilChanged(),
  filter(term => term.length >= 2),
  switchMap(term => this.patientService.search(term)),  // cancels prev HTTP call
  takeUntilDestroyed()
).subscribe(results => this.results = results);
```

### mergeMap — Run All Concurrently
```typescript
// Submit all selected codes in parallel (don't cancel each other)
from(selectedCodes).pipe(
  mergeMap(code => this.codingService.confirmCode(visitId, code))
).subscribe();
```

### concatMap — Queue, Run Sequentially (Order Matters)
```typescript
// Process audit events in strict order
from(auditEvents).pipe(
  concatMap(event => this.auditService.record(event))
).subscribe();
```

### exhaustMap — Ignore New Until Current Completes (Form Submit)
```typescript
// Prevent double-submit: ignore button clicks while HTTP is in flight
this.submitButton.clicks$.pipe(
  exhaustMap(() => this.claimService.submit(this.form.value))
).subscribe();
```

## 2. State Operators
```typescript
// combineLatest — emit when ALL sources emit
combineLatest([patient$, visit$, codes$]).pipe(
  map(([patient, visit, codes]) => ({ patient, visit, codes }))
);

// withLatestFrom — emit with latest value from secondary (don't trigger on secondary)
this.submitAction$.pipe(
  withLatestFrom(this.currentUser$),
  switchMap(([formData, user]) => this.service.save(formData, user.id))
);

// shareReplay — multicast + replay for late subscribers (avoid duplicate HTTP calls)
readonly patients$ = this.http.get<Patient[]>('/api/patients').pipe(
  shareReplay(1)
);
```

## 3. Error Handling
```typescript
this.codingService.suggest(visitId).pipe(
  retry({ count: 2, delay: 1000 }),  // retry twice with 1s delay
  catchError(err => {
    this.errorMessage = 'Failed to load suggestions. Using cached results.';
    return this.cacheService.getCachedSuggestions(visitId); // graceful fallback
  })
);
```

## 4. Unsubscribe Patterns (Memory Leak Prevention)
```typescript
// ✅ BEST: takeUntilDestroyed (Angular 16+)
export class MyComponent {
  constructor() {
    this.service.data$.pipe(
      takeUntilDestroyed()  // automatically completes on component destroy
    ).subscribe(data => this.data = data);
  }
}

// ✅ Async pipe in template (auto-unsubscribes)
// patients$ | async

// ✅ Manual (older approach)
private destroy$ = new Subject<void>();

ngOnDestroy(): void {
  this.destroy$.next();
  this.destroy$.complete();
}

someStream$.pipe(takeUntil(this.destroy$)).subscribe();
```
