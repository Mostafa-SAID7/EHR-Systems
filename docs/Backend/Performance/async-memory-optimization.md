# Async & Memory Optimization Guide

Optimization practices for asynchronous operations, thread pool starvation prevention, and memory/GC management in .NET.

---

## 1. Thread Pool Starvation & Sync-Over-Async

```csharp
// ❌ DEADLOCK / THREAD STARVATION RISK
public IActionResult GetPatient(int id)
{
    var patient = _patientService.GetPatientAsync(id).Result; // Sync-over-async!
    return Ok(patient);
}

// ✅ THREAD EFFICIENT ASYNC
public async Task<IActionResult> GetPatientAsync(int id)
{
    var patient = await _patientService.GetPatientAsync(id);
    return Ok(patient);
}
```

---

## 2. Memory Allocation & GC Pressure Reduction

1. **`ValueTask<T>`**: Use `ValueTask<T>` for methods that frequently complete synchronously (e.g., cached reads) to eliminate `Task` object allocations.
2. **`ArrayPool<T>`**: Rent buffers for temporary serialization/deserialization instead of allocating new `byte[]` arrays on the Large Object Heap (LOH).
3. **`ReadOnlySpan<T>` / `Memory<T>`**: Parse strings and byte streams without allocating sub-strings.
