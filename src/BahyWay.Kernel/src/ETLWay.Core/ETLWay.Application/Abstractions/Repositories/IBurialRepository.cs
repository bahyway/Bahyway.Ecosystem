using ETLWay.Domain.Entities;

namespace ETLWay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Burial entity.
/// PROJECT-SPECIFIC: ✅ ETLWay
/// PATTERN: ✅ Repository pattern for burial records
/// </summary>
public interface IBurialRepository
{
    /// <summary>
    /// Gets a burial record by ID.
    /// </summary>
    Task<Burial?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a burial record by permit number.
    /// </summary>
    Task<Burial?> GetByPermitNumberAsync(
        string permitNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all burial records for a specific deceased person.
    /// </summary>
    Task<IEnumerable<Burial>> GetByDeceasedIdAsync(
        int deceasedId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all burial records for a specific plot.
    /// </summary>
    Task<IEnumerable<Burial>> GetByPlotIdAsync(
        int plotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets burial records by status.
    /// </summary>
    Task<IEnumerable<Burial>> GetByStatusAsync(
        BurialStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets burial records within a date range.
    /// </summary>
    Task<IEnumerable<Burial>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets burial records registered by a specific person.
    /// </summary>
    Task<IEnumerable<Burial>> GetByRegisteredByAsync(
        string registeredBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a burial record with deceased and plot details.
    /// </summary>
    Task<Burial?> GetWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new burial record.
    /// </summary>
    Task AddAsync(Burial burial, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing burial record.
    /// </summary>
    void Update(Burial burial);

    /// <summary>
    /// Deletes a burial record (soft delete).
    /// </summary>
    void Delete(Burial burial);
}
