using AlarmInsight.Application.Abstractions;
using AlarmInsight.Infrastructure.BackgroundJobs;
using AlarmInsight.Infrastructure.Persistence;
using AlarmInsight.Infrastructure.Persistence.Interceptors;
using AlarmInsight.Infrastructure.Persistence.Repositories;
using BahyWay.SharedKernel.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlarmInsight.Infrastructure;

public static class DependencyInjection
{
    // FIX: Added 'this' before IServiceCollection
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AlarmInsightDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("AlarmInsight");
            options.UseNpgsql(connectionString);

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        services.AddSingleton<AuditInterceptor>();
        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        // Register background jobs
        services.AddScoped<DailyResetBackgroundJob>();

        return services;
    }
}