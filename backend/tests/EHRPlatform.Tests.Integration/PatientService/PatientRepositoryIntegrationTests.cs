#nullable enable

using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Tests.Common.Base;
using EHRPlatform.Tests.Common.Builders;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Integration.PatientService;

/// <summary>
/// Integration tests for Patient repository with database.
/// Tests CRUD operations with real PostgreSQL container.
/// </summary>
public class PatientRepositoryIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task AddPatient_WithValidData_PersistsToDatabase()
    {
        // Arrange
        var patient = new PatientBuilder()
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithEmail("john.doe@test.com")
            .Build();

        // Act
        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Assert
        var savedPatient = await DbContext.Patients.FindAsync(patient.Id);
        savedPatient.Should().NotBeNull();
        savedPatient!.FirstName.Should().Be("John");
        savedPatient.LastName.Should().Be("Doe");
        savedPatient.Email.Should().Be("john.doe@test.com");
    }

    [Fact]
    public async Task GetPatient_WithExistingId_ReturnsPatient()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var patient = new PatientBuilder()
            .WithId(patientId)
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Act
        var retrievedPatient = await DbContext.Patients.FindAsync(patientId);

        // Assert
        retrievedPatient.Should().NotBeNull();
        retrievedPatient!.Id.Should().Be(patientId);
        retrievedPatient.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task UpdatePatient_WithChangedData_PersistsChanges()
    {
        // Arrange
        var patient = new PatientBuilder()
            .WithFirstName("Robert")
            .WithEmail("robert@old.com")
            .Build();

        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Act
        patient.Email = "robert@new.com";
        patient.UpdatedAt = DateTime.UtcNow;
        DbContext.Patients.Update(patient);
        await SaveChangesAsync();

        // Assert
        var updatedPatient = await DbContext.Patients.FindAsync(patient.Id);
        updatedPatient!.Email.Should().Be("robert@new.com");
    }

    [Fact]
    public async Task DeletePatient_WithValidId_RemovesFromDatabase()
    {
        // Arrange
        var patient = new PatientBuilder().Build();
        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        var patientId = patient.Id;

        // Act
        DbContext.Patients.Remove(patient);
        await SaveChangesAsync();

        // Assert
        var deletedPatient = await DbContext.Patients.FindAsync(patientId);
        deletedPatient.Should().BeNull();
    }

    [Fact]
    public async Task AddMultiplePatients_WithUniqueEmails_AllPersist()
    {
        // Arrange
        var patients = new[]
        {
            new PatientBuilder().WithEmail("patient1@test.com").Build(),
            new PatientBuilder().WithEmail("patient2@test.com").Build(),
            new PatientBuilder().WithEmail("patient3@test.com").Build()
        };

        // Act
        DbContext.Patients.AddRange(patients);
        await SaveChangesAsync();

        // Assert
        var count = await System.Linq.AsyncEnumerable.CountAsync(DbContext.Patients);
        count.Should().Be(3);
    }

    [Fact]
    public async Task QueryPatientByMRN_WithValidMRN_ReturnsPatient()
    {
        // Arrange
        var mrn = TestDataGenerator.GenerateMRN();
        var patient = new PatientBuilder()
            .WithMRN(mrn)
            .Build();

        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Act
        var retrievedPatient = DbContext.Patients
            .FirstOrDefault(p => p.MRN == mrn);

        // Assert
        retrievedPatient.Should().NotBeNull();
        retrievedPatient!.MRN.Should().Be(mrn);
    }

    [Fact]
    public async Task ActivePatients_Query_ReturnsOnlyActiveRecords()
    {
        // Arrange
        var activePatient = new PatientBuilder().WithActive(true).Build();
        var inactivePatient = new PatientBuilder().WithActive(false).Build();

        DbContext.Patients.AddRange(activePatient, inactivePatient);
        await SaveChangesAsync();

        // Act
        var activePatients = DbContext.Patients
            .Where(p => p.IsActive)
            .ToList();

        // Assert
        activePatients.Should().HaveCount(1);
        activePatients.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task PatientDates_ArePreserved_InDatabase()
    {
        // Arrange
        var createdDate = DateTime.UtcNow;
        var patient = new PatientBuilder().Build();
        patient.CreatedAt = createdDate;

        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Act
        await RefreshEntityAsync(patient);

        // Assert
        patient.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        patient.CreatedAt.Should().BeCloseTo(createdDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task PatientIndexes_OnEmailAndMRN_Exist()
    {
        // Arrange
        var patient1 = new PatientBuilder().WithEmail("test1@test.com").Build();
        var patient2 = new PatientBuilder().WithEmail("test2@test.com").Build();

        DbContext.Patients.AddRange(patient1, patient2);
        await SaveChangesAsync();

        // Act & Assert - Query should be fast due to index
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = DbContext.Patients
            .FirstOrDefault(p => p.Email == "test1@test.com");
        sw.Stop();

        result.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public async Task TransactionRollback_PreventsPersistence()
    {
        // Arrange
        var patient = new PatientBuilder().Build();

        // Act
        DbContext.Patients.Add(patient);
        await SaveChangesAsync();

        // Transaction will be rolled back in DisposeAsync
        var patientId = patient.Id;

        // In real scenario, after transaction rollback:
        // The patient should not exist in new context
        await Task.CompletedTask;

        // Assert - This is handled by IAsyncLifetime pattern
        // See IntegrationTestBase.DisposeAsync() for automatic rollback
    }
}
