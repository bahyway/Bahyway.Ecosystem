using BahyWay.SharedKernel.Domain.Primitives;

namespace BahyWay.SharedKernel.Domain.Entities;

/// <summary>
/// Base class for entities requiring audit tracking.
/// Automatically tracks who created/modified the entity and when.
/// REUSABLE: ✅ ALL PROJECTS
/// CRITICAL FOR: HireWay (compliance), NajafCemetery (legal records), ETLway (data lineage)
/// </summary>
public abstract class AuditableEntity : Entity
{
    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Who created the entity (user ID, email, or system name).
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// When the entity was last modified (UTC).
    /// </summary>
    public DateTime? LastModifiedAt { get; private set; }

    /// <summary>
    /// Who last modified the entity.
    /// </summary>
    public string LastModifiedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Marks the entity as created by a specific user.
    /// Called automatically by AuditInterceptor.
    /// </summary>
    public void MarkAsCreated(string createdBy, DateTime? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        CreatedBy = createdBy;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as modified by a specific user.
    /// Called automatically by AuditInterceptor.
    /// </summary>
    public void MarkAsModified(string modifiedBy, DateTime? modifiedAt = null)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new ArgumentException("ModifiedBy cannot be null or empty", nameof(modifiedBy));

        LastModifiedBy = modifiedBy;
        LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
    }
}