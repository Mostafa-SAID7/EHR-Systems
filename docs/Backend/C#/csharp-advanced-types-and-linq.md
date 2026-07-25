# C# Language & Advanced Types Guide

Guide to C# type system features, modern records, pattern matching, LINQ performance, and memory behavior.

---

## 1. Value Types vs Reference Types & GC

- **Value Types** (`struct`, `int`, `bool`, `DateTime`, `Span<T>`): Allocated on stack or inline within containing object; passed by value.
- **Reference Types** (`class`, `interface`, `string`, `object`): Allocated on Managed Heap; Garbage Collector manages lifecycle.

### Memory Optimization Best Practice
```csharp
// ❌ Avoid Boxing (allocating reference wrapper on Heap for Value Type)
ArrayList list = new ArrayList();
list.Add(42); // Boxing occurs!

// ✅ Use Generic Collections (Zero Boxing)
List<int> genericList = new List<int>();
genericList.Add(42);
```

---

## 2. Records vs Classes (C# 9+)

```csharp
// Record: Value-based equality, immutable by default (with init properties)
public record PatientDto(int Id, string Name, string MedicalRecordNumber);

// Usage with non-destructive mutation (with expression)
var originalPatient = new PatientDto(1, "John Doe", "MRN-100");
var updatedPatient = originalPatient with { Name = "John Smith" };
```

---

## 3. LINQ & Deferred Execution (`IEnumerable` vs `IQueryable`)

- **`IEnumerable<T>`**: In-Memory LINQ evaluation. Executes delegates on objects loaded into app RAM.
- **`IQueryable<T>`**: Out-Of-Memory LINQ evaluation (Entity Framework). Translates Expression Trees into native SQL queries.
