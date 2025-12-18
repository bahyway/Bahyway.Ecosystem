using System;

namespace BahyWay.SharedKernel.Domain.Entities;

/// <summary>
/// Base class for entities that support soft delete with audit tracking.
/// Entities are marked as deleted but not removed from database.
/// REUSABLE: ✅ ALL PROJECTS
/// CRITICAL FOR: NajafCemetery (cannot permanently delete burial records), HireWay (compliance)
/// </summary>
public abstract class SoftDeletableAuditableEntity : AuditableEntity
{
    /// <summary>
    /// Indicates whether the entity has been soft deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// When the entity was deleted (UTC).
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Who deleted the entity.
    /// </summary>
    public string DeletedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Soft deletes the entity.
    /// </summary>
    public void MarkAsDeleted(string deletedBy, DateTime? deletedAt = null)
    {
        if (string.IsNullOrWhiteSpace(deletedBy))
            throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = deletedAt ?? DateTime.UtcNow;

        // Also mark as modified
        MarkAsModified(deletedBy, deletedAt);
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    public void Restore(string restoredBy)
    {
        if (string.IsNullOrWhiteSpace(restoredBy))
            throw new ArgumentException("RestoredBy cannot be null or empty", nameof(restoredBy));

        IsDeleted = false;
        DeletedBy = string.Empty;
        DeletedAt = null;

        MarkAsModified(restoredBy);
    }
}