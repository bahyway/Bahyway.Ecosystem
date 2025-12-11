using ETLWay.Domain.Entities;

namespace ETLWay.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Deceased entity.
/// PROJECT-SPECIFIC: ✅ ETLWay
/// PATTERN: ✅ Repository pattern - reusable across ALL projects
/// </summary>
public interface IDeceasedRepository
{
    /// <summary>
    /// Gets a deceased person by ID.
    /// </summary>
    Task<Deceased?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a deceased person by national ID.
    /// </summary>
    Task<Deceased?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all deceased persons (paginated).
    /// </summary>
    Task<IEnumerable<Deceased>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches deceased persons by name.
    /// </summary>
    Task<IEnumerable<Deceased>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deceased persons who died within a date range.
    /// </summary>
    Task<IEnumerable<Deceased>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new deceased person record.
    /// </summary>
    Task AddAsync(Deceased deceased, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing deceased person record.
    /// </summary>
    void Update(Deceased deceased);

    /// <summary>
    /// Deletes a deceased person record (soft delete).
    /// </summary>
    void Delete(Deceased deceased);
}
