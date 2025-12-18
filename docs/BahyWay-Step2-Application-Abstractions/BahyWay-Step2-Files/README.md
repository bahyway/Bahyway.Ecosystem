# BahyWay Step 2: Application Layer Abstractions

This package contains the essential interface files for your SharedKernel Application layer.

---

## 📁 What's Included

```
BahyWay.SharedKernel/Application/Abstractions/
├── IApplicationLogger.cs         ✅ Structured logging
├── ICacheService.cs               ✅ Redis caching
├── IBackgroundJobService.cs       ✅ Hangfire background jobs
├── IFileStorageService.cs         ✅ File upload/download
└── IFileWatcherService.cs         ✅ File system monitoring (ETLway)
```

---

## 🎯 Installation Steps

### Step 1: Copy Files to Your Project

1. **Extract this ZIP file**
2. **Navigate to:** `C:\Users\Bahaa\source\_OTAP\Dev\Bahyway\src\BahyWay.SharedKernel\`
3. **Copy the entire `Application` folder** into `BahyWay.SharedKernel\`

Your structure should be:
```
BahyWay.SharedKernel/
├── Domain/              (you already have this)
└── Application/         ← ADD THIS
    └── Abstractions/
        ├── IApplicationLogger.cs
        ├── ICacheService.cs
        ├── IBackgroundJobService.cs
        ├── IFileStorageService.cs
        └── IFileWatcherService.cs
```

### Step 2: Include in Visual Studio

1. Open **Visual Studio 2022**
2. Right-click on **BahyWay.SharedKernel** project
3. Click **"Add" → "Existing Folder..."**
4. Select the **Application** folder
5. All 5 files should now appear in Solution Explorer

### Step 3: Build

Press **Ctrl+Shift+B** to build the solution.

**Expected result:** ✅ Build succeeded

---

## 📖 What Each Interface Does

### 1. IApplicationLogger&lt;T&gt;

**Purpose:** Structured logging with correlation IDs

**Used in:** ALL projects, ALL handlers

**Example:**
```csharp
private readonly IApplicationLogger<CreateAlarmCommandHandler> _logger;

public async Task<Result<int>> Handle(...)
{
    _logger.LogInformation("Creating alarm from source: {Source}", request.Source);
    
    // ... your code ...
    
    _logger.LogError(ex, "Failed to create alarm: {Error}", ex.Message);
}
```

**Implementation:** Will be in `Infrastructure/Logging/ApplicationLogger.cs` (next step)

---

### 2. ICacheService

**Purpose:** Redis-based distributed caching

**Used in:** ALL projects (heavy: SmartForesight, SteerView, AlarmInsight)

**Example:**
```csharp
private readonly ICacheService _cache;

public async Task<Result<Alarm>> Handle(...)
{
    // Get from cache or create
    var alarm = await _cache.GetOrCreateAsync(
        CacheKeys.Alarms.ById(alarmId),
        () => _repository.GetByIdAsync(alarmId),
        CacheExpiration.Medium);
    
    // Invalidate cache after update
    await _cache.RemoveByPatternAsync(CacheKeys.Alarms.Pattern());
}
```

**Key Features:**
- `GetOrCreateAsync` - Get cached or compute
- `RemoveByPatternAsync` - Bulk invalidation
- `CacheKeys` helper - Consistent key naming
- `CacheExpiration` - Standard TTL values

**Implementation:** Will be in `Infrastructure/Caching/RedisCacheService.cs` (next step)

---

### 3. IBackgroundJobService

**Purpose:** Async processing with Hangfire

**Used in:** ALL projects (heavy: ETLway file processing, SmartForesight training)

**Example:**
```csharp
private readonly IBackgroundJobService _jobs;

public async Task<Result<int>> Handle(...)
{
    // Save alarm
    await _repository.AddAsync(alarm);
    
    // Enqueue background job (keeps API fast!)
    _jobs.Enqueue(() => ProcessAlarmJob.ExecuteAsync(alarm.Id));
    
    // Schedule for later
    _jobs.Schedule(() => SendReminderJob.Execute(alarm.Id), TimeSpan.FromHours(2));
    
    // Recurring job
    _jobs.AddOrUpdateRecurringJob(
        "cleanup-old-alarms",
        () => CleanupJob.Execute(),
        CronExpressions.DailyAt2AM);
    
    return Result.Success(alarm.Id);
}
```

**Key Features:**
- `Enqueue` - Fire-and-forget
- `Schedule` - Delayed execution
- `AddOrUpdateRecurringJob` - Cron scheduling
- `BaseBackgroundJob` - Base class with logging

**Implementation:** Will be in `Infrastructure/BackgroundJobs/HangfireBackgroundJobService.cs` (next step)

---

### 4. IFileStorageService

**Purpose:** File upload/download abstraction

**Used in:** ETLway, HireWay (resumes), NajafCemetery (documents), SmartForesight (models)

**Example:**
```csharp
private readonly IFileStorageService _fileStorage;

public async Task<Result> Handle(...)
{
    // Upload file
    var uploadResult = await _fileStorage.UploadAsync(
        fileStream,
        "resume.pdf",
        StorageContainers.Resumes);
    
    if (uploadResult.IsFailure)
        return uploadResult.Error;
    
    var filePath = uploadResult.Value;
    
    // Download later
    var downloadResult = await _fileStorage.DownloadAsync(
        filePath,
        StorageContainers.Resumes);
}
```

**Key Features:**
- Upload/Download streams
- List files in container
- Copy/Move files
- Get metadata
- `StorageContainers` - Standard container names

**Implementation:** Will be in `Infrastructure/FileStorage/LocalFileStorageService.cs` (next step)

---

### 5. IFileWatcherService

**Purpose:** Monitor file system for new files

**Used in:** ETLway (PRIMARY), any project with file imports

**Example:**
```csharp
// In your startup/configuration:
_fileWatcher.StartWatching(
    directoryPath: "C:\\ETL\\Inbox",
    fileFilter: "*.zip",
    onFileCreated: (args) => {
        _logger.LogInformation("New file detected: {FileName}", args.FileName);
        
        // Enqueue processing job
        _backgroundJobService.Enqueue(() => 
            ProcessZipFileJob.ExecuteAsync(args.FilePath));
    });
```

**Key Features:**
- Watch for Created/Changed/Deleted events
- File filters (*.zip, *.xml, etc.)
- Stabilization delay (wait for complete write)
- Multiple directory watching

**Implementation:** Will be in `Infrastructure/FileWatcher/FileWatcherService.cs` (next step)

---

## 🎯 Usage in AlarmInsight

Here's how you'll use these in your Command Handler:

```csharp
// File: AlarmInsight.Application/Alarms/Commands/CreateAlarm/CreateAlarmCommandHandler.cs

using MediatR;
using BahyWay.SharedKernel.Domain.Primitives;
using BahyWay.SharedKernel.Application.Abstractions; // ← THESE INTERFACES!
using AlarmInsight.Domain.Aggregates;

public class CreateAlarmCommandHandler : IRequestHandler<CreateAlarmCommand, Result<int>>
{
    private readonly IAlarmRepository _repository;
    private readonly IApplicationLogger<CreateAlarmCommandHandler> _logger; // ← #1
    private readonly ICacheService _cache; // ← #2
    private readonly IBackgroundJobService _jobs; // ← #3

    public CreateAlarmCommandHandler(
        IAlarmRepository repository,
        IApplicationLogger<CreateAlarmCommandHandler> logger,
        ICacheService cache,
        IBackgroundJobService jobs)
    {
        _repository = repository;
        _logger = logger;
        _cache = cache;
        _jobs = jobs;
    }

    public async Task<Result<int>> Handle(
        CreateAlarmCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Log (Correlation ID automatically added)
        _logger.LogInformation("Creating alarm from source: {Source}", request.Source);

        // 2. Create alarm using domain
        var alarmResult = Alarm.Create(
            request.Source,
            request.Description,
            request.Severity,
            request.Location);

        if (alarmResult.IsFailure)
        {
            _logger.LogWarning("Failed to create alarm: {Error}", alarmResult.Error.Message);
            return Result.Failure<int>(alarmResult.Error);
        }

        var alarm = alarmResult.Value;

        // 3. Set audit info
        alarm.MarkAsCreated("System");

        // 4. Save to database
        await _repository.AddAsync(alarm, cancellationToken);

        _logger.LogInformation("Alarm created successfully: {AlarmId}", alarm.Id);

        // 5. Invalidate cache
        await _cache.RemoveByPatternAsync(CacheKeys.Alarms.Pattern());

        // 6. Enqueue background job for processing
        _jobs.Enqueue(() => ProcessAlarmJob.ExecuteAsync(alarm.Id));

        return Result.Success(alarm.Id);
    }
}
```

---

## ✅ What You Can Build Now

With these 5 interfaces, you can now create:

1. ✅ **AlarmInsight.Application** layer
   - Commands (CreateAlarm, ProcessAlarm, ResolveAlarm)
   - Queries (GetAlarm, GetActiveAlarms)
   - Handlers (using the 3 core interfaces)

2. ✅ **Background Jobs**
   - ProcessAlarmJob
   - SendNotificationJob
   - CleanupOldAlarmsJob

---

## ⏳ What's Next (Step 3)

After you add these files and build successfully, you'll need:

### Infrastructure Implementations:

1. `ApplicationLogger.cs` (implements IApplicationLogger)
2. `RedisCacheService.cs` (implements ICacheService)
3. `HangfireBackgroundJobService.cs` (implements IBackgroundJobService)
4. `LocalFileStorageService.cs` (implements IFileStorageService)
5. `FileWatcherService.cs` (implements IFileWatcherService)

**I'll provide these in the next package!**

---

## 🐛 Troubleshooting

### Build Errors

If you get build errors about missing namespaces:

1. **Using statements missing:**
   Add these at the top of files:
   ```csharp
   using BahyWay.SharedKernel.Application.Abstractions;
   using BahyWay.SharedKernel.Domain.Primitives;
   ```

2. **Result type not found:**
   Make sure your `Domain/Primitives/Result.cs` is in the project

---

## 📞 Quick Reference

| Interface | Primary Use | Heavy Users |
|-----------|-------------|-------------|
| IApplicationLogger | Logging | ALL projects |
| ICacheService | Performance | SmartForesight, SteerView |
| IBackgroundJobService | Async processing | ETLway, SmartForesight |
| IFileStorageService | File management | ETLway, HireWay, Cemetery |
| IFileWatcherService | File detection | ETLway (PRIMARY) |

---

## 🎯 Success Criteria

After adding these files, you should be able to:

- ✅ Build SharedKernel successfully
- ✅ Reference these interfaces in AlarmInsight.Application
- ✅ Write command handlers that use logging, caching, and background jobs
- ✅ No compile errors

---

**Questions? Issues? Let me know and I'll help!** 🚀
