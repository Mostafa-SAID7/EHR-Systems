# Database & Query Optimization Guide

Detailed walkthrough of database performance, indexing strategies, query execution plans, and ORM optimization in ASP.NET Core & Entity Framework.

---

## 1. N+1 Query Prevention & Projection

### Entity Framework Core Best Practices

```csharp
// ❌ BAD: Loads all doctors, then fires N queries for appointments
var doctors = await _context.Doctors.ToListAsync();
foreach (var doc in doctors)
{
    var appts = await _context.Appointments.Where(a => a.DoctorId == doc.Id).ToListAsync();
}

// ✅ BETTER: Eager Loading with Include
var doctorsWithAppts = await _context.Doctors
    .Include(d => d.Appointments)
    .AsNoTracking() // Eliminates change tracking overhead for read-only queries
    .ToListAsync();

// 🚀 BEST: Select Projection (Only fetches required fields from SQL)
var doctorSummaries = await _context.Doctors
    .Select(d => new DoctorDto
    {
        Id = d.Id,
        Name = d.Name,
        ActiveAppointmentsCount = d.Appointments.Count(a => a.Status == AppointmentStatus.Confirmed)
    })
    .AsNoTracking()
    .ToListAsync();
```

---

## 2. SQL Indexing & Execution Plans

1. **Covering Indexes**: Create composite indexes covering `(DoctorId, Status, AppointmentDate)` to avoid table scans on frequent queries.
2. **AsNoTracking**: Always apply `.AsNoTracking()` on read queries to save memory allocation in EF Core's ChangeTracker.
3. **Compiled Queries**: Use `EF.CompileAsyncQuery` for hot-path queries that execute thousands of times per second.
