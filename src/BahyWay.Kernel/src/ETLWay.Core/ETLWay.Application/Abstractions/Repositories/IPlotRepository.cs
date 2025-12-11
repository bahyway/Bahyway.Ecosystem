using ETLWay.Domain.Entities;
using ETLWay.Domain.ValueObjects;

namespace ETLWay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Plot entity.
/// PROJECT-SPECIFIC: ✅ ETLWay
/// PATTERN: ✅ Repository pattern with geospatial queries
/// </summary>
public interface IPlotRepository
{
    /// <summary>
    /// Gets a plot by ID.
    /// </summary>
    Task<Plot?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plot by plot number and section.
    /// </summary>
    Task<Plot?> GetByPlotNumberAsync(
        string plotNumber,
        string section,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all plots in a specific section.
    /// </summary>
    Task<IEnumerable<Plot>> GetBySectionAsync(
        string section,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plots by status.
    /// </summary>
    Task<IEnumerable<Plot>> GetByStatusAsync(
        PlotStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available plots for a given plot type.
    /// </summary>
    Task<IEnumerable<Plot>> GetAvailablePlotsAsync(
        PlotType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plots within a geographical area (bounding box).
    /// </summary>
    Task<IEnumerable<Plot>> GetPlotsInAreaAsync(
        decimal minLatitude,
        decimal maxLatitude,
        decimal minLongitude,
        decimal maxLongitude,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plots near a specific location (within radius in meters).
    /// </summary>
    Task<IEnumerable<Plot>> GetPlotsNearLocationAsync(
        GeospatialLocation location,
        int radiusInMeters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new plot.
    /// </summary>
    Task AddAsync(Plot plot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing plot.
    /// </summary>
    void Update(Plot plot);

    /// <summary>
    /// Deletes a plot (soft delete).
    /// </summary>
    void Delete(Plot plot);
}
