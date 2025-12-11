using BahyWay.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AlarmInsight.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically sets audit properties on entities.
/// Sets CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private const string SystemUser = "System"; // TODO: Get from ICurrentUserService

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = dbContext.ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.MarkAsCreated(SystemUser, DateTime.UtcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkAsModified(SystemUser, DateTime.UtcNow);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}