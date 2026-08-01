# Phase 3: Create Service-Specific DbContexts

## Overview

This document explains how to create service-specific DbContext implementations that replace the shared monolithic DbContext.

## Current Problem (BEFORE)

```csharp
// ❌ WRONG: All services share ONE DbContext with ALL entities
using EHRPlatform.Common.Data.Contexts;

// In any service
public class MyService
{
    public MyService(EHRPlatformContext dbContext)
    {
        var patients = dbContext.Patients;      // Access Patient entities
        var appointments = dbContext.Appointments;
        var invoices = dbContext.Invoices;
        // ... can access EVERYTHING, no boundaries
    }
}

// Problem: Services are tightly coupled through shared context
// Problem: Changes to Patient schema affect all services
// Problem: Database migrations must coordinate across all services
```

## Target State (AFTER)

```csharp
// ✅ RIGHT: Each service has its OWN DbContext with only its entities
using EHRPlatform.Services.Patient.Data;

// In Patient Service only
public class PatientService
{
    public PatientService(PatientContext dbContext)
    {
        var patients = dbContext.Patients;              // ✅ Can access
        var contacts = dbContext.PatientContacts;       // ✅ Can access
        var allergies = dbContext.PatientAllergies;     // ✅ Can access
        // var appointments = dbContext.Appointments;   // ❌ Cannot access (doesn't exist)
    }
}

// Result: Services are truly independent
// Result: Patient schema changes don't affect other services
// Result: Each service manages its own migrations
```

---

## Step 1: DbContext Structure Pattern

Every service should follow this structure:

```
src/EHRPlatform.Services.MyService/
├── Domain/
│   └── Entities/
│       ├── MyEntity.cs
│       ├── RelatedEntity.cs
│       └── ValueObject.cs
│
├── Data/
│   ├── MyServiceContext.cs  ← Service-Specific DbContext
│   ├── Migrations/
│   │   └── 20250101_001_baseline.cs
│   └── Configurations/      ← Optional: Entity configurations
│       └── MyEntityConfiguration.cs
│
└── Program.cs               ← Register DbContext here
```

---

## Step 2: DbContext Implementation Pattern

### Basic Template

```csharp
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.YourService.Domain.Entities;

namespace EHRPlatform.Services.YourService.Data
{
    /// <summary>
    /// Service-Specific DbContext for Your Service
    /// This context manages ONLY entities owned by this service.
    /// </summary>
    public class YourServiceContext : DbContext
    {
        public YourServiceContext(DbContextOptions<YourServiceContext> options)
            : base(options)
        {
        }

        // DbSets for this service's entities only
        public DbSet<Entity1> Entity1s { get; set; }
        public DbSet<Entity2> Entity2s { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure only this service's entities
            ConfigureEntity1(modelBuilder);
            ConfigureEntity2(modelBuilder);
        }

        private static void ConfigureEntity1(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity1>(entity =>
            {
                entity.ToTable("Entity1");
                entity.HasKey(e => e.Id);
                // ... configuration
            });
        }

        private static void ConfigureEntity2(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity2>(entity =>
            {
                entity.ToTable("Entity2");
                entity.HasKey(e => e.Id);
                // ... configuration
            });
        }
    }
}
```

---

## Step 3: Register DbContext in Program.cs

### Example: Patient Service

```csharp
using EHRPlatform.Services.Patient.Data;
using EHRPlatform.Common.Data.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register ONLY this service's DbContext
builder.Services.AddDbContext<PatientContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("EHRPlatform.Services.Patient")  // Migrations in this project
    )
);

// ✅ Register migration configuration
var environment = builder.Environment.EnvironmentName;
new MigrationConfiguration(builder.Services)
    .WithEnvironment(environment)
    .AddContext<PatientContext>()
    .Build();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Run migrations for this service
try
{
    await app.Services.RunMigrationsAsync<PatientContext>("PatientService");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to apply migrations");
    if (app.Environment.IsProduction())
        throw;
}

app.Run();
```

---

## Step 4: Entity Configuration Best Practices

### Option A: Fluent Configuration in OnModelCreating

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configure Patient entity
    modelBuilder.Entity<Patient>(entity =>
    {
        entity.ToTable("Patients");
        entity.HasKey(e => e.Id);
        
        // Properties
        entity.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        // Indexes
        entity.HasIndex(e => e.Email)
            .IsUnique()
            .HasName("IX_Patients_Email_Unique");
        
        // Relationships
        entity.HasMany(e => e.Allergies)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Soft delete
        entity.HasQueryFilter(e => e.DeletedAt == null);
    });

    // Configure related entities
    ConfigureAllergy(modelBuilder);
}

private static void ConfigureAllergy(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<PatientAllergy>(entity =>
    {
        entity.ToTable("PatientAllergies");
        entity.HasKey(e => e.Id);
        // ... configuration
    });
}
```

### Option B: Separate Configuration Classes

```csharp
// File: Data/Configurations/PatientConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EHRPlatform.Services.Patient.Domain.Entities;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasName("IX_Patients_Email_Unique");

        // ... rest of configuration
    }
}

// In DbContext:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new PatientConfiguration());
    modelBuilder.ApplyConfiguration(new PatientAllergyConfiguration());
    // ... other configurations
}
```

---

## Step 5: Polyglot Database Support

### PostgreSQL Context (Primary)

```csharp
// PatientContext.cs - PostgreSQL
public class PatientContext : DbContext
{
    // ... DbSets and configuration ...

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // PostgreSQL-specific configuration
        optionsBuilder.UseNpgsql(sql =>
        {
            sql.MigrationsAssembly("EHRPlatform.Services.Patient");
            sql.CommandTimeout(300);
        });
    }
}
```

### MongoDB Support (Document Services)

```csharp
// Clinical Service might use both PostgreSQL and MongoDB

// File: src/EHRPlatform.Services.Clinical/Data/ClinicalContext.cs
public class ClinicalContext : DbContext
{
    // PostgreSQL for structured data
    public DbSet<ClinicalNote> ClinicalNotes { get; set; }
    public DbSet<VitalSigns> VitalSigns { get; set; }

    // ... configuration ...
}

// File: src/EHRPlatform.Services.Clinical/Data/ClinicalDocumentRepository.cs
public class ClinicalDocumentRepository
{
    private readonly IMongoDatabase _database;

    public ClinicalDocumentRepository(IMongoClient client, string databaseName = "ehr_clinical_documents")
    {
        _database = client.GetDatabase(databaseName);
    }

    public async Task SaveDocumentAsync(BsonDocument document)
    {
        var collection = _database.GetCollection<BsonDocument>("ClinicalDocuments");
        await collection.InsertOneAsync(document);
    }

    public async Task<BsonDocument> GetDocumentAsync(ObjectId id)
    {
        var collection = _database.GetCollection<BsonDocument>("ClinicalDocuments");
        return await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id))
            .FirstOrDefaultAsync();
    }
}

// In Program.cs:
var mongoClient = new MongoClient(builder.Configuration["ConnectionStrings:MongoDB"]);
builder.Services.AddScoped(_ => mongoClient);
builder.Services.AddScoped<ClinicalDocumentRepository>();
```

---

## Step 6: Data Seeding

### Seed Data in OnModelCreating

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Seed default roles
    var adminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    var doctorRoleId = Guid.Parse("10000000-0000-0000-0000-000000000002");

    modelBuilder.Entity<Role>().HasData(
        new Role
        {
            Id = adminRoleId,
            Name = "Admin",
            Description = "System administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new Role
        {
            Id = doctorRoleId,
            Name = "Doctor",
            Description = "Healthcare provider",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    );

    // Seed default permissions
    var readPatientPermissionId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    modelBuilder.Entity<Permission>().HasData(
        new Permission
        {
            Id = readPatientPermissionId,
            Name = "read_patient",
            Resource = "Patient",
            Action = "READ",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    );
}
```

---

## Step 7: Handling Shared Infrastructure

### What Should NOT Be in Service-Specific DbContext

```csharp
// ❌ WRONG: Including shared infrastructure entities
public DbSet<AuditLog> AuditLogs { get; set; }  // This is service-specific!
public DbSet<OutboxEvent> OutboxEvents { get; set; }  // This is service-specific!

// If a service needs these, create separate repositories/contexts
```

### Correct Approach: Separate Repositories

```csharp
// File: src/EHRPlatform.Services.Patient/Infrastructure/AuditService.cs
public class AuditService : IAuditService
{
    private readonly IDbConnection _auditConnection;

    public AuditService(IConfiguration config)
    {
        _auditConnection = new NpgsqlConnection(
            config.GetConnectionString("AuditConnection")
        );
    }

    public async Task LogAsync(AuditEntry entry)
    {
        const string sql = @"
            INSERT INTO ""AuditEntries"" 
            (UserId, Action, Entity, OldValues, NewValues, Timestamp)
            VALUES (@userId, @action, @entity, @oldValues, @newValues, @timestamp)";

        using (var cmd = new NpgsqlCommand(sql, _auditConnection))
        {
            cmd.Parameters.AddWithValue("@userId", entry.UserId);
            // ... other parameters
            await _auditConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            _auditConnection.Close();
        }
    }
}
```

---

## Step 8: Testing the DbContext

### Unit Test Example

```csharp
using Microsoft.EntityFrameworkCore;
using Xunit;
using EHRPlatform.Services.Patient.Data;
using EHRPlatform.Services.Patient.Domain.Entities;

public class PatientContextTests
{
    [Fact]
    public async Task Create_Patient_WithAllergies_Success()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PatientContext>()
            .UseInMemoryDatabase("test_db")
            .Options;

        using (var context = new PatientContext(options))
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                MedicalRecordNumber = "MRN001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Status = "Active"
            };

            var allergy = new PatientAllergy
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                AllergenName = "Penicillin",
                Severity = "Severe"
            };

            patient.Allergies.Add(allergy);

            // Act
            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            // Assert
            var savedPatient = await context.Patients
                .Include(p => p.Allergies)
                .FirstOrDefaultAsync(p => p.Id == patient.Id);

            Assert.NotNull(savedPatient);
            Assert.Single(savedPatient.Allergies);
            Assert.Equal("Penicillin", savedPatient.Allergies.First().AllergenName);
        }
    }

    [Fact]
    public async Task Soft_Delete_Filters_Out_Deleted_Patients()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PatientContext>()
            .UseInMemoryDatabase("test_db_soft_delete")
            .Options;

        using (var context = new PatientContext(options))
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                MedicalRecordNumber = "MRN002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                DateOfBirth = new DateTime(1985, 5, 15),
                Status = "Active"
            };

            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            // Act: Soft delete
            patient.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Assert: Patient is filtered out by HasQueryFilter
            var result = await context.Patients
                .FirstOrDefaultAsync(p => p.Id == patient.Id);

            Assert.Null(result);  // Soft deleted patient not found
        }
    }
}
```

---

## Step 9: Migration Setup for Service DbContext

Each service should have its own migrations folder:

```
src/EHRPlatform.Services.Patient/
├── Data/
│   ├── PatientContext.cs
│   └── Migrations/
│       ├── 20250101_001_baseline.cs
│       ├── 20250101_001_baseline.Designer.cs
│       ├── PatientContextModelSnapshot.cs
│       └── README.md (migration documentation)
```

To create a new migration for a service:

```bash
# From root directory
dotnet ef migrations add AddPatientField \
    --project src/EHRPlatform.Services.Patient \
    --context PatientContext \
    --output-dir Data/Migrations
```

---

## Step 10: Verification Checklist

After implementing Phase 3, verify:

- [ ] Each service has its own DbContext (PatientContext, IdentityContext, etc.)
- [ ] DbContext includes ONLY entities owned by that service
- [ ] DbContext is registered in Program.cs
- [ ] MigrationsAssembly points to the service project
- [ ] Migrations are created per-service
- [ ] Service can run independently with its own DbContext
- [ ] No service references another service's DbContext
- [ ] Soft delete filters configured where needed
- [ ] Indexes created for performance-critical queries
- [ ] Foreign keys configured with appropriate cascade behavior
- [ ] Unit tests verify DbContext works correctly

---

## Service-Specific DbContext Checklist

### For Each Service Implementation:

**Identity Service (IdentityContext):**
- [x] User entity + relationships
- [x] Role entity + join table
- [x] Permission entity + join table
- [x] RefreshToken entity
- [x] Default roles seeded
- [x] Default permissions seeded

**Patient Service (PatientContext):**
- [x] Patient entity (master)
- [x] PatientContact entity
- [x] PatientAllergy entity
- [x] PatientCondition entity
- [x] PatientInsurance entity
- [x] PatientEmergencyContact entity
- [x] PatientMedicalHistory entity (1-to-1)
- [x] Soft delete filter

**Clinical Service (ClinicalContext):**
- [ ] ClinicalNote entity
- [ ] VitalSigns entity
- [ ] Diagnosis entity
- [ ] Procedure entity

**Appointment Service (AppointmentContext):**
- [ ] Appointment entity
- [ ] TimeSlot entity
- [ ] AppointmentStatus history

**Notification Service (NotificationContext):**
- [ ] NotificationPreference entity
- [ ] NotificationTemplate entity
- [ ] MessageLog entity

**Audit Service (AuditContext):**
- [ ] AuditEntry entity
- [ ] AccessLog entity
- [ ] TTL configuration for old entries

**Billing Service (BillingContext):**
- [ ] Invoice entity
- [ ] InvoiceLineItem entity
- [ ] Payment entity
- [ ] PaymentMethod entity

**Prescription Service (PrescriptionContext):**
- [ ] Prescription entity
- [ ] PrescriptionLineItem entity
- [ ] RefillRequest entity
- [ ] Medication reference

**Analytics Service (AnalyticsContext):**
- [ ] Report entity
- [ ] ReportExecution entity
- [ ] Metric entity

---

## Summary

**Phase 3 achieves:**
✅ Each service owns its database schema
✅ No coupling through shared DbContext
✅ Independent database migrations per service
✅ Services can deploy independently
✅ Breaking changes are localized

**Before Phase 3:**
- All services shared one monolithic DbContext
- Changes to schema affected all services
- Database migrations had to coordinate across 10 services
- Services tightly coupled at data layer

**After Phase 3:**
- Each service has isolated DbContext
- Schema changes are localized
- Migrations are managed per-service
- Services are true microservices

---

**Phase 3 Status:** Ready for Implementation  
**Files Created:**
- ✅ IdentityContext.cs (complete with seeding)
- ✅ PatientContext.cs (complete with all entities)
- ✅ Phase 3 Documentation (this file)

**Remaining:** Create DbContexts for remaining 8 services following the same pattern.

