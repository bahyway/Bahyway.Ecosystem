using AlarmInsight.Infrastructure;
using AlarmInsight.Infrastructure.BackgroundJobs;
using BahyWay.SharedKernel; // For AddBahyWayPlatform
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Interfaces; // For IMessageBus
//using ETLWay.Logic.Actors; // For IngestZipFileActor
using Akka.Actor; // For ActorSystem
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using global::ETLWay.Logic.Actors; // For IngestZipFileActor and StatisticsActor

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
});

// ============================================
// 4. ADD INFRASTRUCTURE SERVICES
// ============================================
builder.Services.AddInfrastructure(builder.Configuration);

// Logging & Caching
builder.Services.AddSingleton(typeof(IApplicationLogger<>), typeof(BahyWay.SharedKernel.Infrastructure.Logging.ApplicationLogger<>));
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, BahyWay.SharedKernel.Infrastructure.Caching.InMemoryCacheService>();

// =========================================================
// ⭐ 5. NEW: ADD BAHYWAY PLATFORM (REDIS, AKKA, ZIP)
// =========================================================

// A. Register Redis & ZipService (Using the helper we created in SharedKernel)
builder.Services.AddBahyWayPlatform(builder.Configuration);

// B. Register Akka Actor System
builder.Services.AddSingleton<ActorSystem>(_ => ActorSystem.Create("BahyWaySystem"));

// C. Register the WatchDog (The Trigger)
// Ensure FileWatchDogService uses the injected IMessageBus
builder.Services.AddHostedService<FileWatchDogService>();

// ============================================
// 6. ADD HANGFIRE
// ============================================
//var hangfireConn = builder.Configuration.GetConnectionString("AlarmInsight");
////builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(hangfireConn));
//?? "Host=localhost;Port=5432;Database=bahyway_db;Username=bahyway_admin;Password=password123";
//builder.Services.AddHangfireServer();
// Get connection string, OR use the fallback string if it returns null
var hangfireConn = builder.Configuration.GetConnectionString("AlarmInsight")
    ?? "Host=localhost;Port=5432;Database=bahyway_db;Username=bahyway_admin;Password=password123";

// Now use the variable
builder.Services.AddHangfire(config => config.UsePostgreSqlStorage(hangfireConn));
builder.Services.AddHangfireServer();
// ============================================
// 7. ADD CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// =========================================================
// ⭐ 8. NEW: START THE ACTORS (THE "INGEST PROPS" PART)
// =========================================================
// We need to manually start the actors so they are listening for Redis messages immediately.
using (var scope = app.Services.CreateScope())
{
    var actorSystem = app.Services.GetRequiredService<ActorSystem>();
    var bus = app.Services.GetRequiredService<IMessageBus>();
    var zipService = app.Services.GetRequiredService<IZipExtractionService>();

    // 1. Create Statistics Actor (The Scoreboard)
    var statsProps = Props.Create(() => new StatisticsActor(bus));
    actorSystem.ActorOf(statsProps, "stats");

    // 2. Create Ingest Actor (Passes Bus + ZipWorker)
    var ingestProps = Props.Create(() => new IngestZipFileActor(bus, zipService));
    actorSystem.ActorOf(ingestProps, "ingest"); // This actor is now ALIVE and waiting.

    // 3. NEW: The Pattern-Based Pipeline Actor
    // PASS BOTH SERVICES: ZipService AND Bus

    // Get the Message Resolver
    var messageResolver = app.Services.GetRequiredService<IMessageResolver>();

    //var pipelineProps = Props.Create(() => new GeneratedPipelineActor(zipService, bus));

    var pipelineProps = Props.Create(() => new GeneratedPipelineActor(zipService, bus, messageResolver));
    actorSystem.ActorOf(pipelineProps, "generated_pipeline");
}

// ============================================
// 9. CONFIGURE PIPELINE
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHangfireDashboard();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ============================================
// 10. RUN
// ============================================
app.Run();