using System.Collections.Generic;
using BahyWay.SharedKernel.Domain.Primitives;
using BahyWay.SharedKernel.Domain.ValueObjects;

namespace AlarmInsight.Domain.ValueObjects;

/// <summary>
/// Represents an alarm location with coordinates.
/// PROJECT-SPECIFIC: ✅ AlarmInsight
/// PATTERN: Value Object (reusable pattern)
/// </summary>
public sealed class Location : ValueObject
{
    private Location(string name, double latitude, double longitude)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Name { get; }
    public double Latitude { get; }
    public double Longitude { get; }

    /// <summary>
    /// Creates a location with validation.
    /// </summary>
    public static Result<Location> Create(string? name, double latitude, double longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Location>(
                Error.Validation("Location.NameRequired", "Location name is required"));

        if (latitude < -90 || latitude > 90)
            return Result.Failure<Location>(
                Error.Validation("Location.InvalidLatitude", "Latitude must be between -90 and 90"));

        if (longitude < -180 || longitude > 180)
            return Result.Failure<Location>(
                Error.Validation("Location.InvalidLongitude", "Longitude must be between -180 and 180"));

        return Result.Success(new Location(name, latitude, longitude));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Name;
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Name} ({Latitude}, {Longitude})";
}