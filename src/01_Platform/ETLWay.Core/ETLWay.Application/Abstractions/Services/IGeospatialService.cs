using BahyWay.SharedKernel.Domain.Primitives;
using ETLWay.Domain.ValueObjects;

namespace ETLWay.Application.Abstractions.Services;

/// <summary>
/// Service interface for geospatial operations.
/// REUSABLE: ✅ NajafCemetery, SteerView (geospatial features)
/// INTEGRATES: H3, Leaflet, OpenStreetMap
/// PURPOSE: Provides geospatial calculations and H3 indexing
/// </summary>
public interface IGeospatialService
{
    /// <summary>
    /// Calculates the distance between two geospatial locations in meters.
    /// </summary>
    Task<Result<double>> CalculateDistanceAsync(
        GeospatialLocation location1,
        GeospatialLocation location2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates H3 index for a given latitude and longitude at specified resolution.
    /// H3 Resolution: 9 = ~0.1km^2, 10 = ~0.015km^2, 11 = ~0.002km^2
    /// </summary>
    Task<Result<string>> GenerateH3IndexAsync(
        decimal latitude,
        decimal longitude,
        int resolution = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets neighboring H3 cells for a given H3 index.
    /// Useful for proximity searches.
    /// </summary>
    Task<Result<IEnumerable<string>>> GetNeighboringH3CellsAsync(
        string h3Index,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a location is within the Najaf cemetery boundaries.
    /// </summary>
    Task<Result<bool>> IsWithinCemeteryBoundariesAsync(
        GeospatialLocation location,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the center point of an H3 cell.
    /// </summary>
    Task<Result<GeospatialLocation>> GetH3CellCenterAsync(
        string h3Index,
        CancellationToken cancellationToken = default);
}
