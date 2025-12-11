using AlarmInsight.Infrastructure;
using AlarmInsight.Infrastructure.BackgroundJobs;
using BahyWay.SharedKernel.Application.Abstractions;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. ADD CONTROLLERS & SWAGGER
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AlarmInsight API",
        Version = "v1",
        Description = "Alarm processing and management system"
    });
});

// ============================================
// 2. ADD MEDIATR (Application Layer)
// ============================================
builder.Services.AddMediatR(config =>
{
    // Register all handlers from Application assembly
    config.RegisterServicesFromAssembly(
        typeof(AlarmInsight.Application.Abstractions.IAlarmRepository).Assembly);
});

// ============================================
// 3. ADD DATABASE CONTEXT (Infrastructure Layer)
// ============================================
builder.Services.AddDbContext<AlarmInsight.Infrastructure.Persistence.AlarmInsightDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AlarmInsight")
        ?? "Host=localhost;Port=5432;Database=alarminsight;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString);

    // Enable detailed errors in development
#if DEBUG
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
#endif
});

// ============================================
// 4. ADD INFRASTRUCTURE SERVICES
// ============================================

// Add Infrastructure services (repositories, db context, background jobs)
builder.Services.AddInfrastructure(builder.Configuration);

// ⭐ ADD THESE MISSING SERVICES:

// Add Application Logger
builder.Services.AddSingleton(typeof(BahyWay.SharedKernel.Application.Abstractions.IApplicationLogger<>),
                              typeof(BahyWay.SharedKernel.Infrastructure.Logging.ApplicationLogger<>));

// Add Cache Service (in-memory cache for now)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<BahyWay.SharedKernel.Application.Abstractions.ICacheService,
                              BahyWay.SharedKernel.Infrastructure.Caching.InMemoryCacheService>();

// ADD POSTGRESQL HEALTH SERVICE (uncomment if you want it)
// builder.Services.AddScoped<IPostgreSQLHealthService, PostgreSQLHealthService>();

// ============================================
// 4.5. ADD HANGFIRE (for background jobs)
// ============================================
var connectionString = builder.Configuration.GetConnectionString("AlarmInsight")
    ?? "Host=localhost;Port=5432;Database=alarminsight;Username=postgres;Password=postgres";

builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(connectionString);
});

builder.Services.AddHangfireServer();

// ============================================
// 5. ADD CORS (for frontend development)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
// 6. CONFIGURE MIDDLEWARE PIPELINE
// ============================================

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AlarmInsight API v1");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

// Enable CORS
app.UseCors("AllowAll");

// Enable Hangfire Dashboard and middleware
app.UseHangfireDashboard();

// Enable HTTPS Redirection
app.UseHttpsRedirection();

// Enable Authorization
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// ============================================
// 6.5. SCHEDULE BACKGROUND JOBS
// ============================================

// Schedule the daily 4pm reset job
var backgroundJobService = app.Services.GetRequiredService<IBackgroundJobService>();
backgroundJobService.AddOrUpdateRecurringJob(
    "daily-system-reset",
    // () => app.Services.CreateScope().ServiceProvider.GetRequiredService<DailyResetBackgroundJob>().ExecuteAsync(),
    () => app.Services.CreateScope().ServiceProvider.GetRequiredService<DailyResetBackgroundJob>().ExecuteAsync(System.Threading.CancellationToken.None),
    CronExpressions.DailyAtHour(16)); // 4 PM (16:00)

// ============================================
// 7. RUN THE APPLICATION
// ============================================
app.Run();