# Phase 2: Eliminate Shared Domain Models

## Overview

This document outlines how to decouple services by **eliminating shared domain entities** while preserving the **thin shared infrastructure library**.

## Current Problem (BEFORE)

```
EHRPlatform.Common (Shared)
├── Domain/
│   └── Entities/          ← ALL SERVICES IMPORT THESE
│       ├── Patient.cs     ← Used by Patient, Clinical, Appointment, Billing services
│       ├── Appointment.cs ← Used by Appointment, Clinical, Notification services
│       ├── Prescription.cs
│       ├── Invoice.cs
│       ├── User.cs        ← Used by ALL services
│       └── ... (shared models)
│
├── Data/
│   └── Contexts/          ← Shared DbContext with ALL entities
│       └── BaseDbContext.cs
│
└── Infrastructure/        ← Shared utilities (KEEP THIS)
    ├── Caching/
    ├── Resilience/
    ├── Logging/
    └── CQRS patterns

PROBLEM:
- Services can't evolve schemas independently
- Breaking changes in shared entities affect all services
- Services are tightly coupled through shared models
- Database migrations must coordinate across services
```

## Target State (AFTER)

```
EHRPlatform.Common (Thin Infrastructure Library Only)
├── Domain/
│   └── Constants/         ← ONLY constants, NO entities
│       ├── StatusConstants.cs (AppointmentStatus values only)
│       ├── ErrorCodes.cs
│       └── ... (value objects that don't change)
│
├── Application/
│   └── Common/
│       ├── Behaviors/     ← CQRS pipeline behaviors (LoggingBehavior, etc.)
│       ├── CQRS/          ← Interfaces (ICommand, IQuery, IHandler)
│       └── Validation/
│
├── Infrastructure/        ← Shared utilities (KEEP)
│   ├── Caching/
│   ├── Resilience/        ← Polly policies
│   ├── Logging/           ← Serilog setup
│   └── Migrations/        ← Migration utilities
│
└── Shared/
    └── DTOs/              ← ONLY for inter-service communication
        ├── PatientDto.cs  ← Used ONLY in Kafka events, not database
        ├── AppointmentDto.cs
        └── ... (DTOs for event payloads)

EACH SERVICE (Patient Service as example)
├── src/EHRPlatform.Services.Patient/
│   ├── Domain/
│   │   └── Entities/      ← Patient owns its entities
│   │       ├── Patient.cs (service-specific)
│   │       ├── Allergy.cs
│   │       └── ValueObjects/
│   │
│   ├── Data/
│   │   ├── Contexts/
│   │   │   └── PatientContext.cs (service-specific DbContext)
│   │   └── Migrations/
│   │       └── (service-specific migrations)
│   │
│   └── Application/
│       ├── Features/
│       │   └── GetPatient/
│       └── Mappers/       ← Map to DTOs for events

RESULT:
- Each service owns its domain
- Services communicate via DTOs (Kafka events)
- Breaking changes in Patient schema don't affect other services
- True microservices independence achieved ✅
```

---

## Step 1: Define Which Code to Keep vs Remove

### KEEP in EHRPlatform.Common

```csharp
// ✅ These are infrastructure utilities, not domain logic
├── Application/
│   └── Common/
│       ├── Behaviors/          // MediatR pipelines
│       │   ├── LoggingBehavior.cs
│       │   ├── ValidationBehavior.cs
│       │   ├── TransactionBehavior.cs
│       │   └── CachingBehavior.cs
│       ├── CQRS/
│       │   ├── ICommand.cs
│       │   ├── IQuery.cs
│       │   ├── ICommandHandler.cs
│       │   ├── ICommandDispatcher.cs
│       │   ├── IQueryDispatcher.cs
│       │   └── ICachedQuery.cs
│       └── Validation/
│           ├── IValidator.cs
│           └── ValidationResult.cs
│
├── Domain/
│   └── Constants/              // Value constants, no state
│       ├── ErrorCodeConstants.cs
│       ├── StringConstants.cs
│       ├── PasswordConstants.cs
│       ├── SlugConstants.cs
│       ├── EncryptionConstants.cs
│       ├── EntityConfigurationConstants.cs
│       └── RetryPolicyConstants.cs
│
├── Infrastructure/
│   ├── Caching/                // Redis utilities
│   │   ├── ICacheService.cs
│   │   ├── CachingServiceExtensions.cs
│   │   └── Handlers/
│   ├── Resilience/             // Polly policies
│   ├── Logging/                // Serilog setup
│   ├── Migrations/             // Migration utilities (MigrationExtensions, etc.)
│   └── Observability/          // OpenTelemetry setup
│
└── Shared/
    └── DTOs/                   // DTO contracts for inter-service events
        └── (Event payloads, NOT database models)
```

### ❌ REMOVE from EHRPlatform.Common (Move to Services)

```csharp
// ❌ These are domain-specific, services must own them
EHRPlatform.Common/Domain/
├── Entities/
│   ├── Patient.cs              ❌ MOVE to Patient Service
│   ├── Appointment.cs          ❌ MOVE to Appointment Service
│   ├── Prescription.cs         ❌ MOVE to Prescription Service
│   ├── User.cs                 ❌ MOVE to Identity Service
│   ├── ClinicalNote.cs         ❌ MOVE to Clinical Service
│   ├── Invoice.cs              ❌ MOVE to Billing Service
│   ├── Audit.cs                ❌ MOVE to Audit Service
│   └── ... (all entities)
│
EHRPlatform.Common/Data/
├── Contexts/
│   ├── BaseDbContext.cs        ❌ REMOVE (too monolithic)
│   ├── EHRPlatformContext.cs   ❌ REMOVE
│   └── ... (all shared contexts)
│
EHRPlatform.Common/Data/Models/
├── ... (all EF Core model configs)  ❌ MOVE to service-specific contexts
```

---

## Step 2: Create Service-Specific Domain Entities

### Example: Patient Service

**File:** `src/EHRPlatform.Services.Patient/Domain/Entities/Patient.cs`

```csharp
using System;
using System.Collections.Generic;

namespace EHRPlatform.Services.Patient.Domain.Entities
{
    /// Service-Specific Entity (NOT Shared)
    /// This entity belongs ONLY to the Patient Service
    /// Other services cannot reference this directly
    /// They communicate via PatientDto (in Shared/DTOs)
    public class Patient
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }

        // Service-specific relationships
        public ICollection<PatientAllergy> Allergies { get; set; }
        public ICollection<PatientCondition> Conditions { get; set; }
        public PatientMedicalHistory MedicalHistory { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    /// Service-Specific Value Objects
    public class PatientAllergy
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string AllergenName { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }

        public Patient Patient { get; set; }
    }

    public class PatientCondition
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string ConditionName { get; set; }
        public string ICD10Code { get; set; }
        public DateTime CreatedAt { get; set; }

        public Patient Patient { get; set; }
    }

    public class PatientMedicalHistory
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string BloodType { get; set; }
        public string SurgicalHistory { get; set; }

        public Patient Patient { get; set; }
    }
}
```

### Example: Identity Service

**File:** `src/EHRPlatform.Services.Identity/Domain/Entities/User.cs`

```csharp
using System;
using System.Collections.Generic;

namespace EHRPlatform.Services.Identity.Domain.Entities
{
    /// Service-Specific Entity (NOT Shared)
    /// User entity belongs ONLY to Identity Service
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Service-specific relationships
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<RolePermission> Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
    }

    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime AssignedAt { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }
    }

    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public User User { get; set; }
    }
}
```

---

## Step 3: Create Service-Specific DbContext

### Example: Patient Service DbContext

**File:** `src/EHRPlatform.Services.Patient/Data/PatientContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Common.Data.Abstractions;

namespace EHRPlatform.Services.Patient.Data
{
    /// Service-Specific DbContext
    /// This context contains ONLY Patient Service entities
    /// No shared entities, no coupling to other services
    public class PatientContext : DbContext
    {
        public PatientContext(DbContextOptions<PatientContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Entities.Patient> Patients { get; set; }
        public DbSet<PatientAllergy> PatientAllergies { get; set; }
        public DbSet<PatientCondition> PatientConditions { get; set; }
        public DbSet<PatientMedicalHistory> PatientMedicalHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient Entity Configuration
            modelBuilder.Entity<Domain.Entities.Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MedicalRecordNumber)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Status).HasDefaultValue("Active");
                
                entity.HasIndex(e => e.MedicalRecordNumber).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);

                // Soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // PatientAllergy Configuration
            modelBuilder.Entity<PatientAllergy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Allergies)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.PatientId);
            });

            // Similar configurations for other entities...
        }
    }
}
```

### Example: Identity Service DbContext

**File:** `src/EHRPlatform.Services.Identity/Data/IdentityContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Identity.Domain.Entities;

namespace EHRPlatform.Services.Identity.Data
{
    /// Service-Specific DbContext
    /// Identity Service owns its User/Role/Permission schema
    public class IdentityContext : DbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Role Configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Permission Configuration
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // UserRole Configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User).WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Role).WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            });

            // Similar for other entities...
        }
    }
}
```

---

## Step 4: Create Service-Specific DTOs (In Shared.DTOs)

### Inter-Service Communication DTOs (Shared)

**File:** `src/EHRPlatform.Common/Shared/DTOs/PatientDto.cs`

```csharp
using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// Shared DTO for Inter-Service Communication
    /// Used ONLY for Kafka event payloads
    /// NOT mapped to Patient Service's internal Entity
    public class PatientDto
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// Event: Patient Created
    public class PatientCreatedEvent
    {
        public Guid PatientId { get; set; }
        public PatientDto PatientData { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    /// Event: Patient Updated
    public class PatientUpdatedEvent
    {
        public Guid PatientId { get; set; }
        public PatientDto PatientData { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
```

**File:** `src/EHRPlatform.Common/Shared/DTOs/UserDto.cs`

```csharp
using System;

namespace EHRPlatform.Common.Shared.DTOs
{
    /// Shared DTO for Inter-Service Communication
    /// Used ONLY for authentication/authorization
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Roles { get; set; }
        public string[] Permissions { get; set; }
    }

    /// Event: User Created
    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public UserDto UserData { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
```

---

## Step 5: Update Service to Use Local DbContext

### Patient Service Program.cs

**Before (Shared DbContext):**
```csharp
// ❌ WRONG: Using shared DbContext
builder.Services.AddDbContext<EHRPlatformContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**After (Service-Specific DbContext):**
```csharp
// ✅ RIGHT: Using service-specific DbContext
builder.Services.AddDbContext<PatientContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("EHRPlatform.Services.Patient")));

// Register migration configuration
var environment = builder.Environment.EnvironmentName;
new MigrationConfiguration(builder.Services)
    .WithEnvironment(environment)
    .AddContext<PatientContext>()
    .Build();
```

---

## Step 6: Mapping Between DTOs and Entities

### Patient Service Mapper

**File:** `src/EHRPlatform.Services.Patient/Application/Mappers/PatientMapper.cs`

```csharp
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Patient.Domain.Entities;

namespace EHRPlatform.Services.Patient.Application.Mappers
{
    /// Maps between internal Entity and shared DTO
    public class PatientMapper
    {
        /// Entity -> DTO (for Kafka events)
        public static PatientDto ToDto(Patient entity)
        {
            return new PatientDto
            {
                Id = entity.Id,
                MedicalRecordNumber = entity.MedicalRecordNumber,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DateOfBirth = entity.DateOfBirth,
                Email = entity.Email,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };
        }

        /// DTO -> Entity (when receiving events from other services)
        public static Patient FromDto(PatientDto dto)
        {
            return new Patient
            {
                Id = dto.Id,
                MedicalRecordNumber = dto.MedicalRecordNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt
            };
        }
    }
}
```

---

## Step 7: Verification Checklist

After Phase 2, verify:

- [ ] EHRPlatform.Common has NO entity classes
- [ ] EHRPlatform.Common has NO DbContext (except migration utilities)
- [ ] Each service has its own Domain/Entities folder
- [ ] Each service has its own DbContext (PatientContext, IdentityContext, etc.)
- [ ] Each service has its own migrations folder
- [ ] Services communicate via DTOs only (no entity sharing)
- [ ] Services can have different schema versions without breaking others
- [ ] All CQRS infrastructure is shared (ICommand, IQuery, etc.)
- [ ] All cache/logging/resilience utilities are shared

---

## Step 8: Breaking Changes Management

### Scenario: Clinical Service Needs Patient Information

**OLD APPROACH (PROBLEM):**
```csharp
// ❌ WRONG: Direct entity reference
using EHRPlatform.Common.Domain.Entities;

var clinicalService = new ClinicalService(dbContext);
var patient = dbContext.Patients.Find(patientId);  // Direct access!
var note = clinicalService.CreateNote(patient);
```

**NEW APPROACH (CORRECT):**
```csharp
// ✅ RIGHT: DTO via event/API
using EHRPlatform.Common.Shared.DTOs;

// Option 1: Call Patient Service API
var patientDto = await patientServiceClient.GetPatientAsync(patientId);
var note = clinicalService.CreateNote(patientDto);

// Option 2: Listen to PatientCreated event
public async Task HandlePatientCreatedAsync(PatientCreatedEvent @event)
{
    var clinicalNote = new ClinicalNote
    {
        PatientId = @event.PatientData.Id,
        PatientName = @event.PatientData.FirstName + " " + @event.PatientData.LastName
    };
    await clinicalContext.ClinicalNotes.AddAsync(clinicalNote);
    await clinicalContext.SaveChangesAsync();
}
```

---

## Implementation Checklist

### For Each Service

- [ ] Create `Domain/Entities/` folder with service-specific entities
- [ ] Create `Domain/ValueObjects/` folder for value objects
- [ ] Create `Data/YourServiceContext.cs` (service-specific DbContext)
- [ ] Move entity model configurations from EHRPlatform.Common
- [ ] Create `Application/Mappers/` to map Entity → DTO
- [ ] Update `Program.cs` to use service-specific DbContext
- [ ] Create migrations for service-specific database
- [ ] Update CQRS handlers to work with local entities
- [ ] Replace any references to shared entities with DTOs

### For EHRPlatform.Common

- [ ] Remove all entity classes from `Domain/Entities/`
- [ ] Remove shared `DbContext` and keep only migration utilities
- [ ] Keep all `Infrastructure/` folder utilities
- [ ] Keep all `Application/Common/` CQRS patterns
- [ ] Create `Shared/DTOs/` folder for inter-service DTOs
- [ ] Document which classes are infrastructure vs deprecated

---

## Migration Path (Phase 2 Steps)

1. ✅ Define what stays vs goes in EHRPlatform.Common
2. ⏭️ **Create service-specific Domain/Entities for each service**
3. ⏭️ **Create service-specific DbContext for each service**
4. ⏭️ **Create mappers (Entity ↔ DTO)**
5. ⏭️ **Update Program.cs in each service**
6. ⏭️ **Move entity model configurations to services**
7. ⏭️ **Test each service independently**
8. ⏭️ **Remove from EHRPlatform.Common**

---

## Benefits After Phase 2

✅ **True Microservices Independence**
- Each service evolves its schema independently
- Breaking changes in Patient schema don't affect Clinical service
- Services can use different versions of entities

✅ **Clear Ownership**
- Patient Service owns Patient entity
- Identity Service owns User entity
- No ambiguity about entity ownership

✅ **Easier Onboarding**
- New developers only understand one service's domain
- No need to learn 10 different entity models

✅ **Better Testing**
- Unit tests test service-specific logic
- No cross-service test dependencies

✅ **Scalability**
- Teams can work on services independently
- No merge conflicts in shared entity files

---

**Phase 2 Status:** Ready for Implementation  
**Estimated Duration:** 4-6 hours (per-service work)  
**Risk Level:** Medium (requires refactoring but backward compatible during migration)  
**Rollback Complexity:** Low (can run both old and new temporarily)

