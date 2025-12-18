namespace ETLWay.Application.Abstractions;

/// <summary>
/// Unit of Work pattern for managing database transactions.
/// REUSABLE: ✅ Pattern reusable across ALL projects
/// PATTERN: Unit of Work from Martin Fowler's Enterprise Application Architecture
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current unit of work to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
