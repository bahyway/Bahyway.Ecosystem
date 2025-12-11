using System.Collections.Generic;
using BahyWay.SharedKernel.Domain.Primitives;

namespace BahyWay.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a physical address value object.
/// REUSABLE: ✅ HireWay (candidate address), NajafCemetery (location), SteerView (geolocation)
/// </summary>
public sealed class Address : ValueObject
{
    private Address(
        string street,
        string city,
        string state,
        string country,
        string postalCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string PostalCode { get; }

    public static Result<Address> Create(
        string? street,
        string? city,
        string? state,
        string? country,
        string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>(
                Error.Validation("Address.StreetRequired", "Street is required"));

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>(
                Error.Validation("Address.CityRequired", "City is required"));

        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address>(
                Error.Validation("Address.CountryRequired", "Country is required"));

        return Result.Success(new Address(
            street,
            city,
            state ?? string.Empty,
            country,
            postalCode ?? string.Empty));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString() =>
        $"{Street}, {City}, {State} {PostalCode}, {Country}";
}