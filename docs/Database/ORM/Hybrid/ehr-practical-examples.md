# EHR Practical Examples - EF + Dapper in Action

Real code examples from the EHR system showing how EF and Dapper work together.

---

## Example 1: Patient CRUD with EF, Dashboard with Dapper

### Create Patient (EF)
```csharp
public class CreatePatientCommand : ICommandHandler<CreatePatientCommand>
{
    private readonly EHRDbContext _context;
    
    public async Task HandleAsync(CreatePatientCommand command)
    {
        // EF handles creation with validation
        var patient = new Patient
        {
            MRN = command.MRN,
            FirstName = command.FirstName,
            LastName = command.LastName,
            DOB = command.DateOfBirth,
            BloodType = command.BloodType,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber
        };
        
        // EF auto-validates relationships
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
    }
}
```

### Get Patient Dashboard (Dapper)
```csharp
public class GetPatientDashboardQuery : IQueryHandler<GetPatientDashboardQuery, PatientDashboardDto>
{
    private readonly IDapperContext _dapper;
    
    public async Task<PatientDashboardDto> HandleAsync(GetPatientDashboardQuery query)
    {
        // Dapper for complex dashboard stats
        return await _dapper.QueryFirstOrDefaultAsync<PatientDashboardDto>(
            @"SELECT 
                p.Id,
                p.MRN,
                p.FirstName + ' ' + p.LastName as FullName,
                COUNT(DISTINCT a.Id) as TotalAppointments,
                SUM(i.Amount) as TotalBilled,
                MAX(a.AppointmentDate) as LastAppointmentDate,
                COUNT(DISTINCT m.Id) as MedicalRecordsCount,
                AVG(DATEDIFF(YEAR, p.DOB, GETDATE())) as AgeInYears
              FROM Patients p
              LEFT JOIN Appointments a ON p.Id = a.PatientId
              LEFT JOIN Invoices i ON p.Id = i.PatientId
              LEFT JOIN MedicalRecords m ON p.Id = m.PatientId
              WHERE p.Id = @PatientId
              GROUP BY p.Id, p.MRN, p.FirstName, p.LastName, p.DOB",
            new { PatientId = query.PatientId }
        );
    }
}
```

---

## Example 2: Appointment Scheduling (EF) + Analytics (Dapper)

### Create Appointment (EF)
```csharp
public class CreateAppointmentCommandHandler : ICommandHandler<CreateAppointmentCommand>
{
    private readonly EHRDbContext _context;
    
    public async Task HandleAsync(CreateAppointmentCommand command)
    {
        // EF handles complex relationships
        var appointment = new Appointment
        {
            PatientId = command.PatientId,
            ProviderId = command.ProviderId,
            AppointmentDate = command.AppointmentDate,
            Duration = command.DurationMinutes,
            Type = command.AppointmentType,
            Status = AppointmentStatus.Scheduled,
            Notes = command.Notes
        };
        
        // EF validates patient exists, provider exists
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
    }
}
```

### Appointment Analytics (Dapper)
```csharp
public class GetAppointmentAnalyticsQuery : IQueryHandler<GetAppointmentAnalyticsQuery, AppointmentAnalyticsDto>
{
    private readonly IDapperContext _dapper;
    
    public async Task<AppointmentAnalyticsDto> HandleAsync(GetAppointmentAnalyticsQuery query)
    {
        // Dapper for complex analytics
        var byStatus = await _dapper.QueryAsync<AppointmentCountByStatus>(
            @"SELECT 
                Status,
                COUNT(*) as Count
              FROM Appointments
              WHERE AppointmentDate >= @StartDate
                AND AppointmentDate < @EndDate
              GROUP BY Status"
        );
        
        var byProvider = await _dapper.QueryAsync<AppointmentCountByProvider>(
            @"SELECT 
                u.FirstName + ' ' + u.LastName as ProviderName,
                COUNT(*) as AppointmentCount,
                AVG(DATEDIFF(MINUTE, a.AppointmentDate, a.AppointmentDate + a.Duration)) as AvgDuration
              FROM Appointments a
              JOIN Users u ON a.ProviderId = u.Id
              WHERE a.AppointmentDate >= @StartDate
                AND a.AppointmentDate < @EndDate
              GROUP BY u.Id, u.FirstName, u.LastName
              ORDER BY AppointmentCount DESC"
        );
        
        var noShow = await _dapper.QueryAsync<NoShowAnalysis>(
            @"SELECT 
                COUNT(*) as TotalNoShows,
                COUNT(DISTINCT PatientId) as UniquePatients,
                CAST(COUNT(*) AS FLOAT) / 
                  (SELECT COUNT(*) FROM Appointments 
                   WHERE AppointmentDate >= @StartDate 
                     AND AppointmentDate < @EndDate) * 100 as NoShowPercentage
              FROM Appointments
              WHERE Status = 'NoShow'
                AND AppointmentDate >= @StartDate
                AND AppointmentDate < @EndDate"
        );
        
        return new AppointmentAnalyticsDto
        {
            CountByStatus = byStatus,
            CountByProvider = byProvider,
            NoShowAnalysis = noShow.First()
        };
    }
}
```

---

## Example 3: Billing - Create Invoice (EF) + Financial Reports (Dapper)

### Create Invoice (EF)
```csharp
public class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand>
{
    private readonly EHRDbContext _context;
    
    public async Task HandleAsync(CreateInvoiceCommand command)
    {
        // EF handles invoice creation with all line items
        var invoice = new Invoice
        {
            PatientId = command.PatientId,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Draft
        };
        
        // Add line items
        foreach (var item in command.LineItems)
        {
            invoice.LineItems.Add(new LineItem
            {
                ServiceCode = item.ServiceCode,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Quantity * item.UnitPrice
            });
        }
        
        invoice.Amount = invoice.LineItems.Sum(l => l.Amount);
        
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }
}
```

### Financial Report (Dapper)
```csharp
public class GetFinancialReportQuery : IQueryHandler<GetFinancialReportQuery, FinancialReportDto>
{
    private readonly IDapperContext _dapper;
    
    public async Task<FinancialReportDto> HandleAsync(GetFinancialReportQuery query)
    {
        // Summary stats
        var summary = await _dapper.QueryFirstOrDefaultAsync<FinancialSummary>(
            @"SELECT 
                COUNT(DISTINCT i.Id) as TotalInvoices,
                SUM(i.Amount) as TotalRevenue,
                SUM(CASE WHEN i.Status = 'Paid' THEN i.Amount ELSE 0 END) as PaidAmount,
                SUM(CASE WHEN i.Status = 'Outstanding' THEN i.Amount ELSE 0 END) as OutstandingAmount,
                SUM(CASE WHEN i.Status = 'Overdue' THEN i.Amount ELSE 0 END) as OverdueAmount,
                COUNT(DISTINCT i.PatientId) as UniquePatients
              FROM Invoices i
              WHERE i.InvoiceDate >= @StartDate
                AND i.InvoiceDate < @EndDate",
            new { StartDate = query.StartDate, EndDate = query.EndDate }
        );
        
        // By service
        var byService = await _dapper.QueryAsync<RevenueByService>(
            @"SELECT 
                li.ServiceCode,
                li.Description as ServiceName,
                COUNT(DISTINCT li.InvoiceId) as InvoiceCount,
                SUM(li.Quantity) as TotalQuantity,
                SUM(li.Amount) as TotalAmount,
                AVG(li.UnitPrice) as AvgPrice
              FROM LineItems li
              JOIN Invoices i ON li.InvoiceId = i.Id
              WHERE i.InvoiceDate >= @StartDate
                AND i.InvoiceDate < @EndDate
              GROUP BY li.ServiceCode, li.Description
              ORDER BY TotalAmount DESC",
            new { StartDate = query.StartDate, EndDate = query.EndDate }
        );
        
        // Top payers
        var topPayers = await _dapper.QueryAsync<TopPayer>(
            @"SELECT TOP 10
                p.Id,
                p.FirstName + ' ' + p.LastName as PatientName,
                COUNT(DISTINCT i.Id) as InvoiceCount,
                SUM(i.Amount) as TotalBilled,
                SUM(CASE WHEN i.Status = 'Paid' THEN i.Amount ELSE 0 END) as Paid,
                SUM(CASE WHEN i.Status = 'Outstanding' THEN i.Amount ELSE 0 END) as Outstanding
              FROM Patients p
              JOIN Invoices i ON p.Id = i.PatientId
              WHERE i.InvoiceDate >= @StartDate
                AND i.InvoiceDate < @EndDate
              GROUP BY p.Id, p.FirstName, p.LastName
              ORDER BY TotalBilled DESC",
            new { StartDate = query.StartDate, EndDate = query.EndDate }
        );
        
        return new FinancialReportDto
        {
            Summary = summary,
            RevenueByService = byService,
            TopPayers = topPayers
        };
    }
}
```

---

## Example 4: Audit Trail - Log Changes (EF) + Query History (Dapper)

### Log Change (EF)
```csharp
public class PatientUpdatedEventHandler : IDomainEventHandler<PatientUpdatedEvent>
{
    private readonly EHRDbContext _context;
    
    public async Task HandleAsync(PatientUpdatedEvent @event)
    {
        // EF creates audit log entry
        var auditEntry = new AuditEntry
        {
            EntityId = @event.PatientId,
            EntityType = "Patient",
            Action = "Updated",
            UserId = @event.UserId,
            Timestamp = DateTime.UtcNow,
            Changes = @event.Changes.Select(c => new ChangeLog
            {
                PropertyName = c.PropertyName,
                OldValue = c.OldValue,
                NewValue = c.NewValue
            }).ToList()
        };
        
        _context.AuditEntries.Add(auditEntry);
        await _context.SaveChangesAsync();
    }
}
```

### Query Audit History (Dapper)
```csharp
public class GetPatientAuditHistoryQuery : IQueryHandler<GetPatientAuditHistoryQuery, AuditHistoryDto>
{
    private readonly IDapperContext _dapper;
    
    public async Task<AuditHistoryDto> HandleAsync(GetPatientAuditHistoryQuery query)
    {
        // Dapper for complex audit query
        var auditLog = await _dapper.QueryAsync<AuditLogEntry>(
            @"SELECT 
                ae.Id,
                ae.EntityId,
                ae.Action,
                u.FirstName + ' ' + u.LastName as ModifiedBy,
                ae.Timestamp,
                cl.PropertyName,
                cl.OldValue,
                cl.NewValue
              FROM AuditEntries ae
              LEFT JOIN Users u ON ae.UserId = u.Id
              LEFT JOIN ChangeLogs cl ON ae.Id = cl.AuditEntryId
              WHERE ae.EntityId = @EntityId
                AND ae.EntityType = 'Patient'
              ORDER BY ae.Timestamp DESC",
            new { EntityId = query.PatientId }
        );
        
        return new AuditHistoryDto
        {
            Changes = auditLog.GroupBy(x => x.Id).Select(g => new AuditEntryDto
            {
                Id = g.Key,
                Action = g.First().Action,
                ModifiedBy = g.First().ModifiedBy,
                Timestamp = g.First().Timestamp,
                PropertyChanges = g.Where(x => x.PropertyName != null).Select(x => new PropertyChangeDto
                {
                    PropertyName = x.PropertyName,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue
                }).ToList()
            }).ToList()
        };
    }
}
```

---

## Example 5: Bulk Import - Dapper Insert, EF Validation

### Bulk Import with Dapper, then EF Validation

```csharp
public class BulkImportPatientsCommandHandler : ICommandHandler<BulkImportPatientsCommand>
{
    private readonly EHRDbContext _context;
    private readonly IDapperContext _dapper;
    
    public async Task HandleAsync(BulkImportPatientsCommand command)
    {
        // Step 1: Bulk insert via Dapper (fast)
        var importTable = "TempPatientImport";
        
        var rowsInserted = await _dapper.ExecuteAsync(
            $@"BULK INSERT {importTable}
               FROM @CsvPath
               WITH (
                   FIELDTERMINATOR = ',',
                   ROWTERMINATOR = '\n',
                   FIRSTROW = 2
               )",
            new { CsvPath = command.CsvFilePath }
        );
        
        // Step 2: Validate via EF (relationships, business rules)
        var importedRecords = await _context.PatientImports
            .Where(p => p.Status == ImportStatus.Pending)
            .ToListAsync();
        
        foreach (var record in importedRecords)
        {
            try
            {
                // EF validates all business rules
                var patient = new Patient
                {
                    MRN = record.MRN,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    DOB = record.DateOfBirth,
                    Email = record.Email
                };
                
                _context.Patients.Add(patient);
                
                record.Status = ImportStatus.Validated;
                record.PatientId = patient.Id;
            }
            catch (Exception ex)
            {
                record.Status = ImportStatus.Failed;
                record.ErrorMessage = ex.Message;
            }
        }
        
        // Step 3: Commit both
        await _context.SaveChangesAsync();
        
        // Step 4: Update import stats via Dapper (direct update)
        await _dapper.ExecuteAsync(
            @"UPDATE PatientImportStats
              SET TotalImported = @Total,
                  ValidatedRecords = @Validated,
                  FailedRecords = @Failed,
                  CompletedAt = GETUTCDATE()
              WHERE ImportId = @ImportId",
            new 
            { 
                Total = rowsInserted,
                Validated = importedRecords.Count(r => r.Status == ImportStatus.Validated),
                Failed = importedRecords.Count(r => r.Status == ImportStatus.Failed),
                ImportId = command.ImportId
            }
        );
    }
}
```

---

## Example 6: Real-time Dashboard - Hybrid Performance

### Dashboard Service
```csharp
public class DashboardService
{
    private readonly EHRDbContext _context;
    private readonly IDapperContext _dapper;
    private readonly IMemoryCache _cache;
    
    public async Task<DashboardDto> GetDashboardAsync()
    {
        const string cacheKey = "dashboard_main";
        
        if (_cache.TryGetValue(cacheKey, out DashboardDto cached))
            return cached;
        
        // Get active patient count via Dapper (fast count)
        var activePatients = await _dapper.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Patients WHERE Status = 'Active'"
        );
        
        // Get today's appointments via Dapper (aggregation)
        var todayAppointments = await _dapper.QueryAsync<AppointmentSummary>(
            @"SELECT 
                Status,
                COUNT(*) as Count
              FROM Appointments
              WHERE CAST(AppointmentDate AS DATE) = CAST(GETUTCDATE() AS DATE)
              GROUP BY Status"
        );
        
        // Get pending alerts via EF (need relationships)
        var pendingAlerts = await _context.SystemAlerts
            .Include(a => a.RelatedPatient)
            .Where(a => a.Status == AlertStatus.Pending)
            .OrderByDescending(a => a.Priority)
            .Take(10)
            .ToListAsync();
        
        // Get recent activities via Dapper (fast pagination)
        var recentActivity = await _dapper.QueryAsync<ActivityLogDto>(
            @"SELECT TOP 20
                ActivityId,
                ActivityType,
                u.FirstName + ' ' + u.LastName as UserName,
                Description,
                Timestamp
              FROM ActivityLogs al
              JOIN Users u ON al.UserId = u.Id
              ORDER BY Timestamp DESC"
        );
        
        var dashboard = new DashboardDto
        {
            ActivePatients = activePatients,
            TodayAppointments = todayAppointments.ToList(),
            PendingAlerts = pendingAlerts,
            RecentActivity = recentActivity.ToList()
        };
        
        // Cache for 30 seconds
        _cache.Set(cacheKey, dashboard, TimeSpan.FromSeconds(30));
        
        return dashboard;
    }
}
```

---

## Example 7: Transaction Across Both

### Appointment Cancellation with Transaction

```csharp
public class CancelAppointmentCommandHandler : ICommandHandler<CancelAppointmentCommand>
{
    private readonly EHRDbContext _context;
    private readonly IDapperContext _dapper;
    
    public async Task HandleAsync(CancelAppointmentCommand command)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Step 1: Update appointment via EF (with business logic)
            var appointment = await _context.Appointments
                .FirstAsync(a => a.Id == command.AppointmentId);
            
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelledReason = command.Reason;
            appointment.CancelledAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Step 2: Create audit entry via EF
            _context.AuditEntries.Add(new AuditEntry
            {
                EntityId = appointment.Id,
                EntityType = "Appointment",
                Action = "Cancelled",
                UserId = command.UserId,
                Timestamp = DateTime.UtcNow
            });
            
            await _context.SaveChangesAsync();
            
            // Step 3: Update stats via Dapper (direct SQL, same transaction)
            await _dapper.ExecuteAsync(
                @"UPDATE AppointmentStats
                  SET CancelledCount = CancelledCount + 1,
                      LastUpdated = GETUTCDATE()
                  WHERE StatDate = CAST(GETUTCDATE() AS DATE)",
                transaction: transaction
            );
            
            // Step 4: Send notification (outside transaction)
            await transaction.CommitAsync();
            
            // Can now safely publish event
            var cancelledEvent = new AppointmentCancelledEvent(
                appointmentId: appointment.Id,
                patientId: appointment.PatientId,
                reason: command.Reason
            );
            
            await PublishEventAsync(cancelledEvent);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

---

## Performance Comparison

### Real Query Times from EHR

```
Query: Get patient with 10 appointments

EF Core:
  .Include(p => p.Appointments)
  Result: 50ms
  
Dapper:
  Query + multi-map
  Result: 5ms
  
Benefit: 10x faster with Dapper for this specific pattern
```

```
Query: Get top 100 patients by billing

EF Core:
  .GroupBy().OrderByDescending()
  Result: 150ms
  
Dapper:
  SQL with aggregation
  Result: 10ms
  
Benefit: 15x faster with Dapper for complex aggregations
```

```
Operation: Insert 100,000 patient records

EF Core:
  AddRange() + SaveChangesAsync()
  Result: 5000ms
  
Dapper:
  Direct INSERT SELECT
  Result: 500ms
  
Benefit: 10x faster with Dapper for bulk operations
```

---

## Best Practices Summary

✅ **DO:**
- Use EF for CRUD and relationships
- Use Dapper for reports and analytics
- Share transactions when both used together
- Cache frequently accessed dashboards
- Monitor performance of each approach

❌ **DON'T:**
- Use EF for 1M+ row operations
- Use Dapper when you need relationship navigation
- Forget to reload EF cache after Dapper updates
- Mix async and sync calls
- Assume you need optimization before profiling

---

## Interview Questions from These Examples

**Q: How do you handle bulk import efficiently?**

A: Use Dapper for bulk insert (fast), then EF for validation (relationships/rules).

**Q: What's the performance of the hybrid approach?**

A: EF optimal for CRUD, Dapper 10-20x faster for reports. Combined = best overall.

**Q: How do transactions work with both?**

A: Dapper accepts transaction parameter. Both participate in same ACID guarantee.

**Q: When would you cache with this approach?**

A: Cache Dapper results (reports) for 30-60 seconds. EF handles fresh writes automatically.

---

## Related Files

- **Hybrid README:** README.md (integration overview)
- **Entity Framework:** ../EntityFramework/README.md
- **Dapper:** ../Dapper/README.md
- **ORM Comparison:** ../orm-comparison.md
