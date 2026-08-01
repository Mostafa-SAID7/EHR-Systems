using System;

namespace EHRPlatform.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical address.
/// </summary>
public class Address : ValueObject
{
    public string StreetAddress { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string streetAddress, string city, string state, string postalCode, string country)
    {
        StreetAddress = streetAddress;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// Create address with validation.
    /// </summary>
    public static Result<Address> Create(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure("Street address is required");

        if (string.IsNullOrWhiteSpace(city))
            return Result<Address>.Failure("City is required");

        if (string.IsNullOrWhiteSpace(state))
            return Result<Address>.Failure("State is required");

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result<Address>.Failure("Postal code is required");

        if (string.IsNullOrWhiteSpace(country))
            return Result<Address>.Failure("Country is required");

        return Result<Address>.Success(new Address(street, city, state, postalCode, country));
    }

    /// <summary>
    /// Get full address as single string.
    /// </summary>
    public string GetFullAddress()
    {
        return $"{StreetAddress}, {City}, {State} {PostalCode}, {Country}";
    }

    protected override System.Collections.Generic.IEnumerable<object?> GetAtomicValues()
    {
        yield return StreetAddress;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => GetFullAddress();
}
