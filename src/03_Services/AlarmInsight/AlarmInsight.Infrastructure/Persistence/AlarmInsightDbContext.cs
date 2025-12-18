using AlarmInsight.Domain.Aggregates;
using AlarmInsight.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace AlarmInsight.Infrastructure.Persistence;

/// <summary>
/// Database context for AlarmInsight.
/// Configures all entities and their relationships.
/// </summary>
public class AlarmInsightDbContext : DbContext
{
    private readonly AuditInterceptor _auditInterceptor;

    public AlarmInsightDbContext(
        DbContextOptions<AlarmInsightDbContext> options,
        AuditInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    // DbSets
    public DbSet<Alarm> Alarms => Set<Alarm>();
    public DbSet<AlarmNote> AlarmNotes => Set<AlarmNote>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add audit interceptor
        optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlarmInsightDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}