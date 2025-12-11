using BahyWay.SharedKernel.Domain.Primitives;

namespace ETLWay.Domain.ValueObjects;

/// <summary>
/// Represents a geospatial location with latitude, longitude, and H3 index.
/// REUSABLE: ✅ NajafCemetery, SteerView (geospatial features)
/// PATTERN: Value object for geospatial data
/// INTEGRATES: Leaflet, OpenStreetMap, H3 hexagon system
/// </summary>
public sealed class GeospatialLocation : ValueObject
{
    private GeospatialLocation() { } // EF Core

    private GeospatialLocation(decimal latitude, decimal longitude, string? h3Index = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        H3Index = h3Index;
    }

    /// <summary>
    /// Latitude coordinate (WGS84).
    /// </summary>
    public decimal Latitude { get; private set; }

    /// <summary>
    /// Longitude coordinate (WGS84).
    /// </summary>
    public decimal Longitude { get; private set; }

    /// <summary>
    /// H3 geospatial index for hexagonal hierarchical spatial indexing.
    /// Used for proximity queries and area analysis.
    /// </summary>
    public string? H3Index { get; private set; }

    /// <summary>
    /// Factory method to create a geospatial location.
    /// </summary>
    public static Result<GeospatialLocation> Create(decimal latitude, decimal longitude, string? h3Index = null)
    {
        if (latitude < -90 || latitude > 90)
            return Result.Failure<GeospatialLocation>(GeospatialLocationErrors.InvalidLatitude);

        if (longitude < -180 || longitude > 180)
            return Result.Failure<GeospatialLocation>(GeospatialLocationErrors.InvalidLongitude);

        return Result.Success(new GeospatialLocation(latitude, longitude, h3Index));
    }

    /// <summary>
    /// Creates a location with H3 index for Najaf cemetery area (example).
    /// Najaf coordinates: approximately 32.0° N, 44.3° E
    /// </summary>
    public static Result<GeospatialLocation> CreateForNajaf(decimal latitude, decimal longitude, string h3Index)
    {
        // Validate Najaf area (rough boundaries)
        if (latitude < 31.5m || latitude > 32.5m)
            return Result.Failure<GeospatialLocation>(GeospatialLocationErrors.OutsideNajafArea);

        if (longitude < 43.5m || longitude > 45.0m)
            return Result.Failure<GeospatialLocation>(GeospatialLocationErrors.OutsideNajafArea);

        if (string.IsNullOrWhiteSpace(h3Index))
            return Result.Failure<GeospatialLocation>(GeospatialLocationErrors.H3IndexRequired);

        return Result.Success(new GeospatialLocation(latitude, longitude, h3Index));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
        yield return H3Index;
    }

    public override string ToString()
    {
        return $"Lat: {Latitude}, Lon: {Longitude}" + (H3Index != null ? $", H3: {H3Index}" : string.Empty);
    }
}

/// <summary>
/// Domain errors for GeospatialLocation.
/// </summary>
public static class GeospatialLocationErrors
{
    public static readonly Error InvalidLatitude = new("GeospatialLocation.InvalidLatitude", "Latitude must be between -90 and 90 degrees");
    public static readonly Error InvalidLongitude = new("GeospatialLocation.InvalidLongitude", "Longitude must be between -180 and 180 degrees");
    public static readonly Error OutsideNajafArea = new("GeospatialLocation.OutsideNajafArea", "Location is outside the Najaf cemetery area");
    public static readonly Error H3IndexRequired = new("GeospatialLocation.H3IndexRequired", "H3 index is required for Najaf locations");
}
