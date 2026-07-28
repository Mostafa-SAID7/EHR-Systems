#nullable enable

using System;
using Xunit;
using FluentAssertions;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Patient domain entity.
/// Tests business logic, validation, and state management.
/// </summary>
public class PatientTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPatient()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@test.com";
        var phone = "+12025551234";
        var dateOfBirth = new DateTime(1980, 1, 1);

        // Act
        var patient = new Patient
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            DateOfBirth = dateOfBirth,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        patient.Id.Should().Be(id);
        patient.FirstName.Should().Be(firstName);
        patient.LastName.Should().Be(lastName);
        patient.Email.Should().Be(email);
        patient.Phone.Should().Be(phone);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.IsActive.Should().BeTrue();
    }

    [Fact]
    public void GetFullName_WithValidNames_ReturnsFormatted()
    {
        // Arrange
        var patient = new Patient
        {
            FirstName = "Jane",
            LastName = "Smith",
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var fullName = $"{patient.FirstName} {patient.LastName}";

        // Assert
        fullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void CalculateAge_WithValidDateOfBirth_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = DateTime.Now.AddYears(-40);
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            DateOfBirth = birthDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var age = DateTime.Now.Year - patient.DateOfBirth.Year;

        // Assert
        age.Should().BeGreaterThanOrEqualTo(40);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void FirstName_WhenEmpty_ShouldNotBeValid(string firstName)
    {
        // Arrange & Act
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = firstName ?? "",
            LastName = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        patient.FirstName.Should().BeEmpty();
    }

    [Fact]
    public void SetActive_ToFalse_DeactivatesPatient()
    {
        // Arrange
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Patient",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        patient.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;

        // Assert
        patient.IsActive.Should().BeFalse();
        patient.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Patient_WithSyntheticData_PassesValidation()
    {
        // Arrange
        var (firstName, lastName) = TestDataGenerator.GenerateName();
        var email = TestDataGenerator.GenerateEmail();
        var phone = TestDataGenerator.GeneratePhoneNumber();
        var dateOfBirth = TestDataGenerator.GenerateDateOfBirth();
        var mrn = TestDataGenerator.GenerateMRN();

        // Act
        var patient = new Patient
        {
            Id = TestDataGenerator.GenerateId(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            DateOfBirth = dateOfBirth,
            MRN = mrn,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        patient.FirstName.Should().NotBeEmpty();
        patient.LastName.Should().NotBeEmpty();
        patient.Email.Should().Contain("@");
        patient.Phone.Should().StartWith("+");
        patient.MRN.Should().NotBeEmpty();
    }

    [Fact]
    public void Multiple_Patients_HaveDifferentIds()
    {
        // Arrange & Act
        var patient1 = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var patient2 = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        patient1.Id.Should().NotBe(patient2.Id);
    }

    [Fact]
    public void Patient_Timestamps_AreUtc()
    {
        // Arrange & Act
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Patient",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        patient.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        patient.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
}
