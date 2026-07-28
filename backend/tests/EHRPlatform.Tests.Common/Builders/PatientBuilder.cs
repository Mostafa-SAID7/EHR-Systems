#nullable enable

using System;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Fluent builder for Patient test entities.
/// </summary>
public class PatientBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "";
    private string _lastName = "";
    private string _email = "";
    private string _phone = "";
    private DateTime _dateOfBirth = DateTime.Now.AddYears(-40);
    private string _mrn = "";
    private string _address = "";
    private string _city = "";
    private string _state = "";
    private string _zipCode = "";
    private string _gender = "M";
    private bool _isActive = true;

    public PatientBuilder()
    {
        var (firstName, lastName) = TestDataGenerator.GenerateName();
        _firstName = firstName;
        _lastName = lastName;
        _email = TestDataGenerator.GenerateEmail();
        _phone = TestDataGenerator.GeneratePhoneNumber();
        _mrn = TestDataGenerator.GenerateMRN();
        var (street, city, state, zip) = TestDataGenerator.GenerateAddress();
        _address = street;
        _city = city;
        _state = state;
        _zipCode = zip;
    }

    public PatientBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PatientBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public PatientBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public PatientBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public PatientBuilder WithPhone(string phone)
    {
        _phone = phone;
        return this;
    }

    public PatientBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dateOfBirth = dateOfBirth;
        return this;
    }

    public PatientBuilder WithMRN(string mrn)
    {
        _mrn = mrn;
        return this;
    }

    public PatientBuilder WithAddress(string street, string city, string state, string zipCode)
    {
        _address = street;
        _city = city;
        _state = state;
        _zipCode = zipCode;
        return this;
    }

    public PatientBuilder WithGender(string gender)
    {
        _gender = gender;
        return this;
    }

    public PatientBuilder WithActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public Patient Build()
    {
        return new Patient
        {
            Id = _id,
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email,
            Phone = _phone,
            DateOfBirth = _dateOfBirth,
            MRN = _mrn,
            Address = _address,
            City = _city,
            State = _state,
            ZipCode = _zipCode,
            Gender = _gender,
            IsActive = _isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
