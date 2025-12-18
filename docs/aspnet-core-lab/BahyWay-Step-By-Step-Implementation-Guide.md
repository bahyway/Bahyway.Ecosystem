# BahyWay - Complete Step-by-Step Implementation Guide
## From Zero to Production-Ready Platform

**Your Setup:**
- 💻 Development: Visual Studio 2022 (Windows Laptop)
- 🐋 Testing/Deployment: VirtualBox Debian 12 VDI with Docker
- 🎯 Goal: Build complete BahyWay ecosystem with shared infrastructure

---

## 🎯 Phase 0: Foundation Setup (Day 1-2)

### Step 1: Create Solution Structure (30 minutes)

```powershell
# Create root directory
mkdir C:\Dev\BahyWay
cd C:\Dev\BahyWay

# Create solution
dotnet new sln -n BahyWay

# Create main folders
mkdir src
mkdir tests
mkdir docs
mkdir scripts
```

### Step 2: Create SharedKernel Project FIRST (1 hour)

**Why Start Here?**
- Establishes patterns for ALL projects
- Avoids duplication
- Forces you to think about reusability

```powershell
# Create SharedKernel Class Library
cd src
dotnet new classlib -n BahyWay.SharedKernel -f net8.0
dotnet sln ..\BahyWay.sln add BahyWay.SharedKernel\BahyWay.SharedKernel.csproj

# Create folder structure in SharedKernel
cd BahyWay.SharedKernel
mkdir Domain\Primitives
mkdir Domain\Entities
mkdir Domain\ValueObjects
mkdir Domain\Events
mkdir Application\Abstractions
mkdir Application\Behaviors
mkdir Application\Exceptions
mkdir Infrastructure\Logging
mkdir Infrastructure\Caching
mkdir Infrastructure\BackgroundJobs
mkdir Infrastructure\Audit
mkdir Infrastructure\FileWatcher
mkdir Infrastructure\FileStorage
mkdir Infrastructure\HealthChecks
```

### Step 3: Install Core NuGet Packages to SharedKernel

```powershell
# Still in SharedKernel directory

# Core packages (USED BY ALL PROJECTS)
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add package Microsoft.Extensions.Logging --version 8.0.0

# Logging (USED BY ALL PROJECTS)
dotnet add package Serilog --version 3.1.1
dotnet add package Serilog.Extensions.Hosting --version 8.0.0
dotnet add package Serilog.Sinks.Console --version 5.0.1
dotnet add package Serilog.Sinks.File --version 5.0.0

# MediatR (USED BY ALL PROJECTS)
dotnet add package MediatR --version 12.2.0

# Validation (USED BY ALL PROJECTS)
dotnet add package FluentValidation --version 11.9.0
```

**IMPORTANT: Mark Reusable Components in Comments**

---

## 🎯 Phase 1: First Project - AlarmInsight (Week 1)

### Why Start With AlarmInsight?

✅ **Moderate complexity** - Not too simple, not too complex
✅ **Real business value** - Actual use case
✅ **Tests all patterns** - CQRS, Rules Engine, Background Jobs, Caching
✅ **Foundation for others** - Patterns apply to all projects

### Step 1: Create AlarmInsight Solution Structure (30 minutes)

```powershell
cd C:\Dev\BahyWay\src

# Create Domain layer
dotnet new classlib -n AlarmInsight.Domain -f net8.0
dotnet sln ..\BahyWay.sln add AlarmInsight.Domain\AlarmInsight.Domain.csproj

# Create Application layer
dotnet new classlib -n AlarmInsight.Application -f net8.0
dotnet sln ..\BahyWay.sln add AlarmInsight.Application\AlarmInsight.Application.csproj

# Create Infrastructure layer
dotnet new classlib -n AlarmInsight.Infrastructure -f net8.0
dotnet sln ..\BahyWay.sln add AlarmInsight.Infrastructure\AlarmInsight.Infrastructure.csproj

# Create Web API
dotnet new webapi -n AlarmInsight.API -f net8.0
dotnet sln ..\BahyWay.sln add AlarmInsight.API\AlarmInsight.API.csproj
```

### Step 2: Add Project References

```powershell
# Domain references SharedKernel
cd AlarmInsight.Domain
dotnet add reference ..\BahyWay.SharedKernel\BahyWay.SharedKernel.csproj

# Application references Domain + SharedKernel
cd ..\AlarmInsight.Application
dotnet add reference ..\AlarmInsight.Domain\AlarmInsight.Domain.csproj
dotnet add reference ..\BahyWay.SharedKernel\BahyWay.SharedKernel.csproj

# Infrastructure references Application + SharedKernel
cd ..\AlarmInsight.Infrastructure
dotnet add reference ..\AlarmInsight.Application\AlarmInsight.Application.csproj
dotnet add reference ..\BahyWay.SharedKernel\BahyWay.SharedKernel.csproj

# API references Infrastructure (gets everything)
cd ..\AlarmInsight.API
dotnet add reference ..\AlarmInsight.Infrastructure\AlarmInsight.Infrastructure.csproj
```

### Step 3: Install AlarmInsight-Specific Packages

```powershell
# Infrastructure packages
cd ..\AlarmInsight.Infrastructure
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
dotnet add package StackExchange.Redis --version 2.7.10
dotnet add package Hangfire.AspNetCore --version 1.8.9
dotnet add package Hangfire.PostgreSql --version 1.20.6

# API packages
cd ..\AlarmInsight.API
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

---

## 🏗️ Phase 2: Build SharedKernel Components (Week 1)

### Priority Order (EXACTLY as you'll need them):

1. **Domain Primitives** (Used by ALL projects)
2. **Logging** (Used by ALL projects)
3. **CQRS Base Classes** (Used by ALL projects)
4. **Result Pattern** (Used by ALL projects)
5. **Audit Entity** (Used by ALL projects)

### Component 1: Entity Base Class (30 minutes)

```csharp
// File: BahyWay.SharedKernel/Domain/Primitives/Entity.cs
// REUSABLE: ✅ ALL PROJECTS (AlarmInsight, ETLway, SmartForesight, HireWay, NajafCemetery, SteerView, SSISight)

namespace BahyWay.SharedKernel.Domain.Primitives;

/// <summary>
/// Base class for all entities in BahyWay ecosystem.
/// Provides identity, equality, and domain event support.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Equals(entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode() * 41;
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
```

### Component 2: AuditableEntity (15 minutes)

```csharp
// File: BahyWay.SharedKernel/Domain/Entities/AuditableEntity.cs
// REUSABLE: ✅ ALL PROJECTS (especially HireWay, NajafCemetery - compliance requirements)

namespace BahyWay.SharedKernel.Domain.Entities;

/// <summary>
/// Base class for entities requiring audit tracking.
/// USED BY: All projects that need to track who/when changes were made.
/// CRITICAL FOR: HireWay (compliance), NajafCemetery (legal records), ETLway (data lineage)
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string LastModifiedBy { get; private set; } = string.Empty;

    public void MarkAsCreated(string createdBy, DateTime? createdAt = null)
    {
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

    public void MarkAsModified(string modifiedBy, DateTime? modifiedAt = null)
    {
        LastModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
    }
}
```

### Component 3: Result Pattern (30 minutes)

```csharp
// File: BahyWay.SharedKernel/Domain/Primitives/Result.cs
// REUSABLE: ✅ ALL PROJECTS - Replaces exceptions for business rule violations

namespace BahyWay.SharedKernel.Domain.Primitives;

/// <summary>
/// Represents the result of an operation.
/// USED BY: ALL projects for returning success/failure without exceptions.
/// PATTERN: Railway-oriented programming
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result");
}

/// <summary>
/// Represents an error with code and message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided");
}
```

---

## 🏗️ Phase 3: Build First Domain (AlarmInsight) (Day 2-3)

### Step 1: Create Alarm Aggregate (1 hour)

```csharp
// File: AlarmInsight.Domain/Aggregates/Alarm.cs
// PROJECT-SPECIFIC: ✅ AlarmInsight only
// PATTERN: ✅ Reusable for other aggregates in all projects

namespace AlarmInsight.Domain.Aggregates;

using BahyWay.SharedKernel.Domain.Entities; // ← USING SHARED
using BahyWay.SharedKernel.Domain.Primitives; // ← USING SHARED

/// <summary>
/// Alarm aggregate root.
/// Represents an alarm in the system with its processing lifecycle.
/// </summary>
public sealed class Alarm : AuditableEntity // ← INHERITS FROM SHARED
{
    private Alarm() { } // EF Core

    private Alarm(
        string source,
        string description,
        AlarmSeverity severity,
        AlarmPriority priority,
        string location)
    {
        Source = source;
        Description = description;
        Severity = severity;
        Priority = priority;
        Location = location;
        Status = AlarmStatus.Pending;
        OccurredAt = DateTime.UtcNow;

        // Raise domain event (shared pattern)
        RaiseDomainEvent(new AlarmCreatedDomainEvent(Id, OccurredAt));
    }

    public string Source { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AlarmSeverity Severity { get; private set; }
    public AlarmPriority Priority { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public AlarmStatus Status { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>
    /// Factory method to create new alarm.
    /// </summary>
    public static Result<Alarm> Create(
        string source,
        string description,
        AlarmSeverity severity,
        AlarmPriority priority,
        string location)
    {
        // Validation using shared Result pattern
        if (string.IsNullOrWhiteSpace(source))
            return Result.Failure<Alarm>(AlarmErrors.SourceRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Alarm>(AlarmErrors.DescriptionRequired);

        var alarm = new Alarm(source, description, severity, priority, location);
        return Result.Success(alarm);
    }

    /// <summary>
    /// Process the alarm.
    /// </summary>
    public Result Process()
    {
        if (Status != AlarmStatus.Pending)
            return Result.Failure(AlarmErrors.AlarmAlreadyProcessed);

        Status = AlarmStatus.Processing;
        ProcessedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AlarmProcessedDomainEvent(Id, ProcessedAt.Value));
        return Result.Success();
    }

    /// <summary>
    /// Resolve the alarm.
    /// </summary>
    public Result Resolve(string resolution)
    {
        if (Status == AlarmStatus.Resolved)
            return Result.Failure(AlarmErrors.AlarmAlreadyResolved);

        if (string.IsNullOrWhiteSpace(resolution))
            return Result.Failure(AlarmErrors.ResolutionRequired);

        Status = AlarmStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AlarmResolvedDomainEvent(Id, ResolvedAt.Value, resolution));
        return Result.Success();
    }
}

// Enums
public enum AlarmSeverity { Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum AlarmPriority { P4 = 4, P3 = 3, P2 = 2, P1 = 1 }
public enum AlarmStatus { Pending, Processing, Resolved, Cancelled }

// Domain Events
public sealed record AlarmCreatedDomainEvent(int AlarmId, DateTime OccurredOn) : IDomainEvent;
public sealed record AlarmProcessedDomainEvent(int AlarmId, DateTime OccurredOn) : IDomainEvent;
public sealed record AlarmResolvedDomainEvent(int AlarmId, DateTime OccurredOn, string Resolution) : IDomainEvent;

// Domain Errors
public static class AlarmErrors
{
    public static readonly Error SourceRequired = new("Alarm.SourceRequired", "Alarm source is required");
    public static readonly Error DescriptionRequired = new("Alarm.DescriptionRequired", "Alarm description is required");
    public static readonly Error AlarmAlreadyProcessed = new("Alarm.AlreadyProcessed", "Alarm has already been processed");
    public static readonly Error AlarmAlreadyResolved = new("Alarm.AlreadyResolved", "Alarm has already been resolved");
    public static readonly Error ResolutionRequired = new("Alarm.ResolutionRequired", "Resolution is required");
}
```

---

## 🏗️ Phase 4: Application Layer (Day 3-4)

### Step 1: Create Command (30 minutes)

```csharp
// File: AlarmInsight.Application/Alarms/Commands/CreateAlarm/CreateAlarmCommand.cs
// PROJECT-SPECIFIC: ✅ AlarmInsight
// PATTERN: ✅ CQRS Command pattern - reusable across ALL projects

namespace AlarmInsight.Application.Alarms.Commands.CreateAlarm;

using MediatR; // ← SHARED via SharedKernel
using BahyWay.SharedKernel.Domain.Primitives; // ← SHARED

/// <summary>
/// Command to create a new alarm.
/// PATTERN: CQRS Command - Used by ALL projects
/// </summary>
public sealed record CreateAlarmCommand(
    string Source,
    string Description,
    int Severity,
    int Priority,
    string Location
) : IRequest<Result<int>>; // Returns Result from SharedKernel
```

### Step 2: Create Command Handler (1 hour)

```csharp
// File: AlarmInsight.Application/Alarms/Commands/CreateAlarm/CreateAlarmCommandHandler.cs
// PROJECT-SPECIFIC: ✅ AlarmInsight
// PATTERN: ✅ Handler pattern - reusable across ALL projects

namespace AlarmInsight.Application.Alarms.Commands.CreateAlarm;

using MediatR;
using BahyWay.SharedKernel.Domain.Primitives;
using BahyWay.SharedKernel.Application.Abstractions; // ← SHARED logging
using AlarmInsight.Domain.Aggregates;
using AlarmInsight.Application.Abstractions;

/// <summary>
/// Handles CreateAlarmCommand.
/// USES: IApplicationLogger (shared), ICacheService (shared), IAlarmRepository (project-specific)
/// </summary>
public sealed class CreateAlarmCommandHandler : IRequestHandler<CreateAlarmCommand, Result<int>>
{
    private readonly IAlarmRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationLogger<CreateAlarmCommandHandler> _logger; // ← SHARED
    private readonly ICacheService _cache; // ← SHARED
    private readonly IBackgroundJobService _jobs; // ← SHARED

    public CreateAlarmCommandHandler(
        IAlarmRepository repository,
        IUnitOfWork unitOfWork,
        IApplicationLogger<CreateAlarmCommandHandler> logger,
        ICacheService cache,
        IBackgroundJobService jobs)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
        _jobs = jobs;
    }

    public async Task<Result<int>> Handle(CreateAlarmCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating alarm from source: {Source}", request.Source);

        // Create alarm using domain factory
        var alarmResult = Alarm.Create(
            request.Source,
            request.Description,
            (AlarmSeverity)request.Severity,
            (AlarmPriority)request.Priority,
            request.Location);

        if (alarmResult.IsFailure)
        {
            _logger.LogWarning("Failed to create alarm: {Error}", alarmResult.Error.Message);
            return Result.Failure<int>(alarmResult.Error);
        }

        var alarm = alarmResult.Value;

        // Set audit info (shared pattern)
        alarm.MarkAsCreated("System"); // In real app: get from ICurrentUserService

        // Add to repository
        await _repository.AddAsync(alarm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alarm created successfully: {AlarmId}", alarm.Id);

        // Invalidate cache (shared service)
        await _cache.RemoveByPatternAsync("alarm:*");

        // Enqueue background job for processing (shared service)
        _jobs.Enqueue<ProcessAlarmJob>(job => job.ExecuteAsync(alarm.Id));

        return Result.Success(alarm.Id);
    }
}
```

---

## 🐳 Docker Setup (Debian 12 VDI)

### Step 1: Create docker-compose.yml

```yaml
# File: C:\Dev\BahyWay\docker-compose.yml
# USAGE: Development database + cache + monitoring

version: '3.8'

services:
  # PostgreSQL with PostGIS (USED BY ALL PROJECTS)
  postgres:
    image: postgis/postgis:15-3.3
    container_name: bahyway-postgres
    environment:
      POSTGRES_USER: bahyway_dev
      POSTGRES_PASSWORD: dev_password_2024
      POSTGRES_DB: bahyway_dev
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - bahyway-network

  # Redis (USED BY ALL PROJECTS)
  redis:
    image: redis:7-alpine
    container_name: bahyway-redis
    command: redis-server --appendonly yes --requirepass redis_password_2024
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - bahyway-network

  # RabbitMQ (USED BY: Projects with event bus)
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: bahyway-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: bahyway
      RABBITMQ_DEFAULT_PASS: rabbitmq_password_2024
    ports:
      - "5672:5672"   # AMQP
      - "15672:15672" # Management UI
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - bahyway-network

  # Seq (Logging - USED BY ALL PROJECTS)
  seq:
    image: datalust/seq:latest
    container_name: bahyway-seq
    environment:
      ACCEPT_EULA: Y
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
    networks:
      - bahyway-network

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:
  seq_data:

networks:
  bahyway-network:
    driver: bridge
```

### Step 2: Copy to Debian VDI and Start

```bash
# On Debian 12 VDI
cd /home/bahaa/bahyway
docker-compose up -d

# Verify
docker ps
```

---

## 📝 Connection Strings (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bahyway_alarmsight_dev;Username=bahyway_dev;Password=dev_password_2024",
    "Redis": "localhost:6379,password=redis_password_2024"
  },
  "Serilog": {
    "SeqServerUrl": "http://localhost:5341"
  }
}
```

---

## ✅ Daily Checklist

### Every Morning:
- [ ] Start Docker on Debian VDI
- [ ] Verify all services running: `docker ps`
- [ ] Open Visual Studio 2022
- [ ] Pull latest from Git
- [ ] Run tests

### Before Committing:
- [ ] All tests passing
- [ ] Code formatted
- [ ] XML comments on public APIs
- [ ] Reusability marked in comments

---

## 🎯 Success Criteria

### After Week 1:
- ✅ SharedKernel compiles
- ✅ Can create Entity, use Result pattern
- ✅ Logging working with Seq
- ✅ AlarmInsight domain models done

### After Week 2:
- ✅ AlarmInsight fully working
- ✅ Can create/process alarms via API
- ✅ Background jobs running
- ✅ Caching working

### After Week 3:
- ✅ ETLway working
- ✅ FileWatcher detecting files
- ✅ Comfortable with patterns
- ✅ Ready to replicate to other projects

---

## 🚀 Next Steps

1. **TODAY**: Create solution structure
2. **TODAY**: Build SharedKernel primitives
3. **DAY 2**: Build SharedKernel infrastructure
4. **DAY 3-4**: Build AlarmInsight domain
5. **WEEK 2**: Complete AlarmInsight
6. **WEEK 3**: Build ETLway

**You're ready to start building!** 🎉

---

**Key Principle**: Build SharedKernel first, validate with one project (AlarmInsight), then replicate the pattern to all other projects. Each subsequent project will be faster because patterns are established.
