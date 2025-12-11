# BahyWay - Developer Quick Reference Card
## Your Daily Development Workflow

---

## 🌅 Morning Routine (5 minutes)

### On Debian 12 VDI:
```bash
# Start Docker containers
cd /home/bahaa/bahyway
docker-compose up -d

# Verify all services
docker ps

# Should see:
# - bahyway-postgres (Port 5432)
# - bahyway-redis (Port 6379)
# - bahyway-rabbitmq (Ports 5672, 15672)
# - bahyway-seq (Port 5341)
```

### On Windows Development Machine:
```powershell
# Open Visual Studio 2022
# Open solution: C:\Dev\BahyWay\BahyWay.sln

# Pull latest changes
git pull origin main

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

### Quick Health Check:
- ✅ PostgreSQL: http://localhost:5432 (use pgAdmin)
- ✅ Redis: `redis-cli -a redis_password_2024 ping` → Should return "PONG"
- ✅ RabbitMQ UI: http://localhost:15672 (bahyway / rabbitmq_password_2024)
- ✅ Seq Logs: http://localhost:5341

---

## 🏗️ When Creating a New Component

### 1. Determine Reusability First

Ask yourself:
- ❓ **Will this be used by multiple projects?**
  - YES → Put in `BahyWay.SharedKernel`
  - NO → Put in project-specific folder

- ❓ **Is this domain logic or infrastructure?**
  - Domain → `Domain/` folder
  - Infrastructure → `Infrastructure/` folder
  - Application → `Application/` folder

### 2. Add Reusability Comments

```csharp
/// <summary>
/// Description of what this does.
/// REUSABLE: ✅ ALL PROJECTS
/// </summary>
// or
/// <summary>
/// Description of what this does.
/// REUSABLE: ✅ ETLway, HireWay, NajafCemetery (file processing)
/// PROJECT-SPECIFIC: ❌
/// </summary>
// or
/// <summary>
/// Description of what this does.
/// PROJECT-SPECIFIC: ✅ AlarmInsight only
/// PATTERN: ✅ Can be adapted for other projects
/// </summary>
```

### 3. Reference Pattern

```
PROJECT-SPECIFIC Entity
↓ (inherits from)
SHARED AuditableEntity
↓ (inherits from)
SHARED Entity
↓ (implements)
SHARED IDomainEvent support
```

---

## 📂 Solution Navigation

### When Working on AlarmInsight:

```
BahyWay.SharedKernel/          ← Reference this for SHARED components
├── Domain/Primitives/         ← Entity, Result, ValueObject
├── Domain/Entities/           ← AuditableEntity
├── Application/Abstractions/  ← IApplicationLogger, ICacheService, etc.
└── Infrastructure/            ← Implementations

AlarmInsight.Domain/           ← PROJECT-SPECIFIC domain logic
├── Aggregates/
│   └── Alarm.cs              ← Uses: AuditableEntity (SHARED)
├── ValueObjects/
└── Events/

AlarmInsight.Application/      ← PROJECT-SPECIFIC use cases
├── Alarms/Commands/
│   └── CreateAlarm/
│       ├── CreateAlarmCommand.cs      ← Uses: IRequest (SHARED)
│       └── CreateAlarmCommandHandler.cs ← Uses: IApplicationLogger (SHARED)
└── Alarms/Queries/

AlarmInsight.Infrastructure/   ← PROJECT-SPECIFIC infrastructure
├── Persistence/
│   ├── AlarmDbContext.cs     ← Uses: AuditInterceptor (SHARED)
│   └── Repositories/
└── DependencyInjection.cs

AlarmInsight.API/              ← PROJECT-SPECIFIC API
└── Program.cs                ← Uses: ALL SHARED infrastructure setup
```

---

## 🔄 Common Patterns Reference

### Pattern 1: Creating an Entity

```csharp
// File: YourProject.Domain/Aggregates/YourEntity.cs
using BahyWay.SharedKernel.Domain.Entities;     // ← SHARED
using BahyWay.SharedKernel.Domain.Primitives;   // ← SHARED

public sealed class YourEntity : AuditableEntity  // ← SHARED base class
{
    private YourEntity() { } // EF Core constructor

    // Factory method with Result pattern (SHARED)
    public static Result<YourEntity> Create(/* params */)
    {
        // Validation
        if (/* invalid */)
            return Result.Failure<YourEntity>(YourErrors.SomeError);

        var entity = new YourEntity(/* ... */);
        return Result.Success(entity);
    }

    // Domain methods
    public Result DoSomething()
    {
        // Business logic

        // Raise domain event (SHARED pattern)
        RaiseDomainEvent(new SomethingHappenedEvent(Id, DateTime.UtcNow));

        return Result.Success();
    }
}
```

### Pattern 2: Creating a Command

```csharp
// File: YourProject.Application/Features/Commands/YourCommand.cs
using MediatR;                                    // ← SHARED via SharedKernel
using BahyWay.SharedKernel.Domain.Primitives;   // ← SHARED

/// <summary>
/// Command to create a new alarm.
/// PATTERN: CQRS Command - Used by ALL projects
/// </summary>
public sealed record YourCommand(
    // Parameters
) : IRequest<Result<int>>;  // ← SHARED Result pattern
```

### Pattern 3: Creating a Command Handler

```csharp
// File: YourProject.Application/Features/Commands/YourCommandHandler.cs
using MediatR;
using BahyWay.SharedKernel.Application.Abstractions;  // ← SHARED

public sealed class YourCommandHandler : IRequestHandler<YourCommand, Result<int>>
{
    private readonly IYourRepository _repository;  // Project-specific
    private readonly IApplicationLogger<YourCommandHandler> _logger;  // ← SHARED
    private readonly ICacheService _cache;  // ← SHARED
    private readonly IBackgroundJobService _jobs;  // ← SHARED

    public async Task<Result<int>> Handle(YourCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Handling {Command}", nameof(YourCommand));

        // 1. Create domain entity
        var entityResult = YourEntity.Create(/* ... */);
        if (entityResult.IsFailure)
            return Result.Failure<int>(entityResult.Error);

        var entity = entityResult.Value;

        // 2. Set audit info (SHARED pattern)
        entity.MarkAsCreated("System");

        // 3. Persist
        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Invalidate cache (SHARED service)
        await _cache.RemoveByPatternAsync($"yourkey:{entity.Id}");

        // 5. Enqueue background job if needed (SHARED service)
        _jobs.Enqueue<YourJob>(job => job.ExecuteAsync(entity.Id));

        return Result.Success(entity.Id);
    }
}
```

### Pattern 4: Setting up Infrastructure (Program.cs)

```csharp
// File: YourProject.API/Program.cs
using Serilog;
using BahyWay.SharedKernel.Infrastructure.Logging;      // ← SHARED
using BahyWay.SharedKernel.Infrastructure.BackgroundJobs;  // ← SHARED

// 1. Logging (SHARED - ALL PROJECTS)
builder.Host.UseSerilog((context, services, configuration) =>
{
    SerilogConfiguration.ConfigureBahyWayLogging(
        context.Configuration,
        "YourProjectName",  // ← Change this
        context.HostEnvironment);
});

// 2. Core services (SHARED - ALL PROJECTS)
builder.Services.AddSingleton<ICorrelationIdService, CorrelationIdService>();
builder.Services.AddScoped(typeof(IApplicationLogger<>), typeof(ApplicationLogger<>));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. Database (SHARED pattern, project-specific context)
builder.Services.AddDbContext<YourDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);

    // Add audit interceptor (SHARED)
    var currentUser = sp.GetRequiredService<ICurrentUserService>();
    var logger = sp.GetRequiredService<IApplicationLogger<AuditInterceptor>>();
    options.AddInterceptors(new AuditInterceptor(currentUser, logger));
});

// 4. Caching (SHARED - ALL PROJECTS)
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// 5. Background Jobs (SHARED - ALL PROJECTS)
builder.Services.ConfigureBahyWayHangfire(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    "YourProjectName");  // ← Change this

// 6. Health Checks (SHARED - ALL PROJECTS)
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnection);
```

---

## 🎯 Where Does Each Component Go?

### SHARED Components (BahyWay.SharedKernel)

| Component | Location | Used By |
|-----------|----------|---------|
| Entity | `Domain/Primitives/Entity.cs` | ALL |
| Result | `Domain/Primitives/Result.cs` | ALL |
| ValueObject | `Domain/Primitives/ValueObject.cs` | ALL |
| AuditableEntity | `Domain/Entities/AuditableEntity.cs` | ALL (especially HireWay, NajafCemetery) |
| IApplicationLogger | `Application/Abstractions/IApplicationLogger.cs` | ALL |
| ICacheService | `Application/Abstractions/ICacheService.cs` | ALL |
| IBackgroundJobService | `Application/Abstractions/IBackgroundJobService.cs` | ALL |
| IFileStorageService | `Application/Abstractions/IFileStorageService.cs` | ETLway, HireWay, NajafCemetery, SmartForesight |
| IFileWatcherService | `Application/Abstractions/IFileWatcherService.cs` | ETLway (primary) |
| Logging implementations | `Infrastructure/Logging/` | ALL |
| Cache implementations | `Infrastructure/Caching/` | ALL |
| Background job implementations | `Infrastructure/BackgroundJobs/` | ALL |
| Audit interceptor | `Infrastructure/Audit/` | ALL |

### PROJECT-SPECIFIC Components

| Component | Location | Project |
|-----------|----------|---------|
| Alarm aggregate | `AlarmInsight.Domain/Aggregates/Alarm.cs` | AlarmInsight |
| CreateAlarmCommand | `AlarmInsight.Application/Commands/CreateAlarm/` | AlarmInsight |
| AlarmDbContext | `AlarmInsight.Infrastructure/Persistence/` | AlarmInsight |
| AlarmController | `AlarmInsight.API/Controllers/` | AlarmInsight |

---

## 🧪 Testing Checklist

### Before Every Commit:

```powershell
# 1. Build
dotnet build

# 2. Run tests
dotnet test

# 3. Check test coverage (if configured)
dotnet test /p:CollectCoverage=true

# 4. Code formatting
dotnet format
```

### Manual Testing:

```bash
# 1. Check logs in Seq
http://localhost:5341

# 2. Check background jobs in Hangfire
http://localhost:5000/hangfire

# 3. Check health
curl http://localhost:5000/health

# 4. Test Redis
redis-cli -a redis_password_2024
> ping
PONG
```

---

## 🐛 Common Issues & Solutions

### Issue: "Cannot connect to PostgreSQL"
```bash
# On Debian VDI:
docker ps | grep postgres
docker logs bahyway-postgres

# Restart if needed:
docker-compose restart postgres
```

### Issue: "Redis connection failed"
```bash
# Check Redis is running:
docker ps | grep redis
redis-cli -a redis_password_2024 ping

# Should return: PONG
```

### Issue: "Hangfire jobs not processing"
```bash
# Check Hangfire dashboard:
http://localhost:5000/hangfire

# Check database connection
# Verify Hangfire schema exists
```

### Issue: "Logs not appearing in Seq"
```bash
# Check Seq is running:
docker ps | grep seq

# Check Seq URL in appsettings:
"SeqServerUrl": "http://localhost:5341"

# Visit Seq UI:
http://localhost:5341
```

---

## 📊 Performance Monitoring

### What to Monitor Daily:

1. **Seq Logs** (http://localhost:5341)
   - Any ERROR level logs?
   - Response times > 500ms?
   - Any exceptions?

2. **Hangfire Dashboard** (http://localhost:5000/hangfire)
   - Failed jobs?
   - Processing time?
   - Queue depth?

3. **Health Checks** (http://localhost:5000/health)
   - All green?
   - Response time?

---

## 🔐 Connection Strings Reference

### Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bahyway_[PROJECT]_dev;Username=bahyway_dev;Password=dev_password_2024",
    "Redis": "localhost:6379,password=redis_password_2024"
  },
  "Serilog": {
    "SeqServerUrl": "http://localhost:5341"
  },
  "Hangfire": {
    "DashboardPath": "/hangfire"
  }
}
```

Replace `[PROJECT]` with: alarmsight, etlway, smartforesight, hireway, najafcemetery, steerview, or ssisight

---

## 🎯 Daily Goals

### Morning (2 hours):
- [ ] Review overnight logs in Seq
- [ ] Check failed Hangfire jobs
- [ ] Review Git changes from team
- [ ] Plan today's work

### Coding Session (4 hours):
- [ ] Write code following patterns
- [ ] Add XML comments
- [ ] Mark reusability in comments
- [ ] Write unit tests

### Afternoon (2 hours):
- [ ] Integration testing
- [ ] Code review
- [ ] Update documentation
- [ ] Commit & push

### End of Day (30 min):
- [ ] All tests passing
- [ ] Logs clean in Seq
- [ ] Health checks green
- [ ] Document any blockers

---

## 🚀 Quick Commands

### Start Everything:
```bash
# Debian VDI
docker-compose up -d

# Windows
# Open Visual Studio 2022
# Press F5 to run
```

### Stop Everything:
```bash
# Debian VDI
docker-compose down
```

### Reset Database:
```bash
# CAREFUL: Deletes all data!
docker-compose down -v
docker-compose up -d
```

### View Logs:
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f postgres
docker-compose logs -f redis
```

---

## 📞 Need Help?

### Documentation:
1. This Quick Reference (you are here!)
2. Step-by-Step Implementation Guide
3. SharedKernel Package Documentation
4. Project-specific README files

### Tools:
- Seq: http://localhost:5341 (logs)
- Hangfire: http://localhost:5000/hangfire (jobs)
- RabbitMQ: http://localhost:15672 (messages)
- Health: http://localhost:5000/health

---

**Print this and keep it by your desk!** 📌
