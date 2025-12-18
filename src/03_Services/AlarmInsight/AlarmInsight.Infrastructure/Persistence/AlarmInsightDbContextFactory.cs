using AlarmInsight.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AlarmInsight.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations.
/// This allows migrations to work without starting the full application.
/// </summary>
public class AlarmInsightDbContextFactory : IDesignTimeDbContextFactory<AlarmInsightDbContext>
{
    public AlarmInsightDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("AlarmInsight")
            ?? "Host=localhost;Port=5432;Database=alarminsight;Username=postgres;Password=postgres";

        // Build DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<AlarmInsightDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // Create a simple audit interceptor (no dependencies needed)
        var auditInterceptor = new AuditInterceptor();

        // Return DbContext
        return new AlarmInsightDbContext(optionsBuilder.Options, auditInterceptor);
    }
}