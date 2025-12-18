# 🔗 AlarmInsight Fuzzy Logic Integration Guide

Complete guide for integrating the fuzzy logic rules engine with AlarmInsight projects.

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Step-by-Step Integration](#step-by-step-integration)
4. [Domain Layer Integration](#domain-layer-integration)
5. [Application Layer Integration](#application-layer-integration)
6. [Testing Strategy](#testing-strategy)
7. [Performance Optimization](#performance-optimization)
8. [Monitoring & Observability](#monitoring--observability)

---

## 🎯 Overview

This guide shows how to integrate the BahyWay.RulesEngine fuzzy logic system into the AlarmInsight application for intelligent alarm classification and processing.

### Benefits

- **Intelligent Classification:** Automatically classify alarms based on multiple fuzzy criteria
- **Reduced Alert Fatigue:** Better prioritization reduces noise
- **Adaptive Behavior:** Handles uncertainty and imprecise inputs gracefully
- **Maintainable Rules:** Business logic in declarative rules, not code
- **Explainable Decisions:** Track which rules fired and why

---

## 🏗️ Architecture

### Integration Points

```
┌─────────────────────────────────────────────┐
│         AlarmInsight.API                    │
│  - REST Controllers                         │
│  - GraphQL Endpoints                        │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│      AlarmInsight.Application               │
│  - AlarmProcessingService                   │
│  - FuzzyAlarmClassifier (New)              │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│        AlarmInsight.Domain                  │
│  - IFuzzyAlarmClassifier (New)             │
│  - AlarmSeverity enum                       │
│  - AlarmContext value object                │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│         BahyWay.RulesEngine                 │
│  - FuzzyEngine                              │
│  - AlarmClassificationRuleSet               │
└─────────────────────────────────────────────┘
```

---

## 🚀 Step-by-Step Integration

### Step 1: Add Project Reference

```bash
cd src/AlarmInsight.Application
dotnet add reference ../BahyWay.RulesEngine/BahyWay.RulesEngine.csproj

cd ../AlarmInsight.Domain
dotnet add reference ../BahyWay.RulesEngine/BahyWay.RulesEngine.csproj
```

### Step 2: Define Domain Interfaces

Create `src/AlarmInsight.Domain/Services/IFuzzyAlarmClassifier.cs`:

```csharp
namespace AlarmInsight.Domain.Services;

/// <summary>
/// Fuzzy logic-based alarm classifier
/// </summary>
public interface IFuzzyAlarmClassifier
{
    /// <summary>
    /// Classifies an alarm based on fuzzy logic rules
    /// </summary>
    Task<AlarmClassificationResult> ClassifyAsync(AlarmContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets classification with explanation
    /// </summary>
    Task<AlarmClassificationResult> ClassifyWithExplanationAsync(AlarmContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for alarm classification
/// </summary>
public record AlarmContext
{
    public double ErrorFrequency { get; init; }  // 0-100 errors/minute
    public double SystemImpact { get; init; }    // 0-10 impact score
    public double HistoricalPattern { get; init; } // 0-10 pattern score
    public string? AlarmType { get; init; }
    public string? SourceSystem { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result of alarm classification
/// </summary>
public record AlarmClassificationResult
{
    public AlarmSeverity Severity { get; init; }
    public double SeverityScore { get; init; }  // 0-4 raw fuzzy output
    public double Confidence { get; init; }      // 0-1 confidence level
    public List<string>? FiredRules { get; init; }
    public Dictionary<string, double>? InputMemberships { get; init; }
    public string? Explanation { get; init; }
}
```

Update `src/AlarmInsight.Domain/Entities/Alarm.cs` to include severity:

```csharp
public class Alarm : Entity<AlarmId>
{
    public string Message { get; private set; }
    public AlarmSeverity Severity { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string SourceSystem { get; private set; }
    // ... other properties

    public void UpdateSeverity(AlarmSeverity newSeverity)
    {
        Severity = newSeverity;
        // Raise domain event
        RaiseDomainEvent(new AlarmSeverityChangedEvent(Id, newSeverity));
    }
}

public enum AlarmSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
```

### Step 3: Implement Application Service

Create `src/AlarmInsight.Application/Services/FuzzyAlarmClassifier.cs`:

```csharp
using BahyWay.RulesEngine.Interfaces;
using BahyWay.RulesEngine.Examples;
using AlarmInsight.Domain.Services;
using Microsoft.Extensions.Logging;

namespace AlarmInsight.Application.Services;

public class FuzzyAlarmClassifier : IFuzzyAlarmClassifier
{
    private readonly IFuzzyEngine _fuzzyEngine;
    private readonly ILogger<FuzzyAlarmClassifier> _logger;
    private const string RuleSetName = "AlarmClassification";

    public FuzzyAlarmClassifier(
        IFuzzyEngine fuzzyEngine,
        ILogger<FuzzyAlarmClassifier> logger)
    {
        _fuzzyEngine = fuzzyEngine;
        _logger = logger;
    }

    public async Task<AlarmClassificationResult> ClassifyAsync(
        AlarmContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Classifying alarm with context: {@Context}", context);

        var inputs = new Dictionary<string, double>
        {
            ["ErrorFrequency"] = context.ErrorFrequency,
            ["SystemImpact"] = context.SystemImpact,
            ["HistoricalPattern"] = context.HistoricalPattern
        };

        var outputs = await _fuzzyEngine.EvaluateAsync(RuleSetName, inputs);
        var severityScore = outputs["Severity"];

        var result = new AlarmClassificationResult
        {
            Severity = AlarmClassificationExample.MapToSeverity(severityScore),
            SeverityScore = severityScore,
            Confidence = CalculateConfidence(severityScore)
        };

        _logger.LogInformation(
            "Alarm classified as {Severity} with score {Score:F2}",
            result.Severity,
            severityScore
        );

        return result;
    }

    public async Task<AlarmClassificationResult> ClassifyWithExplanationAsync(
        AlarmContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await ClassifyAsync(context, cancellationToken);

        // Build explanation
        var explanation = BuildExplanation(context, result);

        return result with { Explanation = explanation };
    }

    private double CalculateConfidence(double severityScore)
    {
        // Calculate confidence based on how close to boundary values
        // Higher confidence when score is far from boundaries (0.5, 1.5, 2.5)
        var boundaries = new[] { 0.5, 1.5, 2.5, 3.5 };
        var minDistance = boundaries.Min(b => Math.Abs(severityScore - b));

        // Map distance to confidence (0.5 to 1.0)
        return 0.5 + (minDistance / 1.0) * 0.5;
    }

    private string BuildExplanation(AlarmContext context, AlarmClassificationResult result)
    {
        var parts = new List<string>
        {
            $"Classified as {result.Severity} (score: {result.SeverityScore:F2})",
            $"Error Frequency: {context.ErrorFrequency:F1}/min",
            $"System Impact: {context.SystemImpact:F1}/10",
            $"Historical Pattern: {context.HistoricalPattern:F1}/10"
        };

        // Add reasoning
        if (result.Severity == AlarmSeverity.Critical)
        {
            parts.Add("Reason: High severity due to critical impact or high error frequency");
        }
        else if (result.Severity == AlarmSeverity.Error)
        {
            parts.Add("Reason: Moderate to high severity indicators present");
        }
        else if (result.Severity == AlarmSeverity.Warning)
        {
            parts.Add("Reason: Some concerning indicators but not critical");
        }
        else
        {
            parts.Add("Reason: Low severity indicators, informational only");
        }

        return string.Join(". ", parts);
    }
}
```

### Step 4: Update Alarm Processing Service

Update `src/AlarmInsight.Application/Services/AlarmProcessingService.cs`:

```csharp
public class AlarmProcessingService : IAlarmProcessingService
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IFuzzyAlarmClassifier _fuzzyClassifier;
    private readonly ILogger<AlarmProcessingService> _logger;

    public AlarmProcessingService(
        IAlarmRepository alarmRepository,
        IFuzzyAlarmClassifier fuzzyClassifier,
        ILogger<AlarmProcessingService> logger)
    {
        _alarmRepository = alarmRepository;
        _fuzzyClassifier = fuzzyClassifier;
        _logger = logger;
    }

    public async Task<ProcessedAlarmDto> ProcessAlarmAsync(
        CreateAlarmCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing alarm: {Message}", command.Message);

        // Build context for fuzzy classification
        var context = new AlarmContext
        {
            ErrorFrequency = await CalculateErrorFrequency(command),
            SystemImpact = await CalculateSystemImpact(command),
            HistoricalPattern = await CalculateHistoricalPattern(command),
            AlarmType = command.Type,
            SourceSystem = command.Source
        };

        // Classify using fuzzy logic
        var classification = await _fuzzyClassifier.ClassifyAsync(context, cancellationToken);

        // Create alarm entity
        var alarm = new Alarm(
            AlarmId.New(),
            command.Message,
            classification.Severity,
            command.Source,
            DateTime.UtcNow
        );

        // Save
        await _alarmRepository.AddAsync(alarm, cancellationToken);
        await _alarmRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Alarm processed successfully: {AlarmId}, Severity: {Severity}",
            alarm.Id,
            classification.Severity
        );

        return MapToDto(alarm, classification);
    }

    private async Task<double> CalculateErrorFrequency(CreateAlarmCommand command)
    {
        // Query recent errors from same source
        // Return rate per minute (0-100)
        var recentErrors = await _alarmRepository.CountRecentAlarmsAsync(
            command.Source,
            TimeSpan.FromMinutes(5)
        );

        return recentErrors / 5.0; // errors per minute
    }

    private async Task<double> CalculateSystemImpact(CreateAlarmCommand command)
    {
        // Calculate impact based on alarm type and affected systems
        // Return score 0-10
        return command.Type switch
        {
            "DatabaseFailure" => 9.0,
            "ServiceDown" => 8.0,
            "HighLatency" => 5.0,
            "Warning" => 3.0,
            _ => 5.0
        };
    }

    private async Task<double> CalculateHistoricalPattern(CreateAlarmCommand command)
    {
        // Analyze historical frequency
        // Return score 0-10
        var historicalCount = await _alarmRepository.CountHistoricalAlarmsAsync(
            command.Source,
            TimeSpan.FromDays(7)
        );

        return Math.Min(10, historicalCount / 10.0);
    }
}
```

### Step 5: Configure Dependency Injection

Update `src/AlarmInsight.API/Program.cs` or `Startup.cs`:

```csharp
using BahyWay.RulesEngine.Extensions;
using BahyWay.RulesEngine.Examples;

// Add fuzzy engine with alarm classification rules
builder.Services.AddFuzzyEngine(engine =>
{
    var alarmRuleSet = AlarmClassificationExample.CreateAlarmClassificationRuleSet();
    engine.AddRuleSet("AlarmClassification", alarmRuleSet);
});

// Register alarm classifier
builder.Services.AddScoped<IFuzzyAlarmClassifier, FuzzyAlarmClassifier>();
```

---

## 🧪 Testing Strategy

### Unit Tests

```csharp
public class FuzzyAlarmClassifierTests
{
    private readonly IFuzzyEngine _fuzzyEngine;
    private readonly ILogger<FuzzyAlarmClassifier> _logger;
    private readonly FuzzyAlarmClassifier _classifier;

    public FuzzyAlarmClassifierTests()
    {
        _fuzzyEngine = new FuzzyEngine();
        var ruleSet = AlarmClassificationExample.CreateAlarmClassificationRuleSet();
        _fuzzyEngine.AddRuleSet("AlarmClassification", ruleSet);

        _logger = Substitute.For<ILogger<FuzzyAlarmClassifier>>();
        _classifier = new FuzzyAlarmClassifier(_fuzzyEngine, _logger);
    }

    [Fact]
    public async Task Classify_LowFrequencyMinorImpact_ReturnsInfo()
    {
        // Arrange
        var context = new AlarmContext
        {
            ErrorFrequency = 5.0,
            SystemImpact = 1.0,
            HistoricalPattern = 1.0
        };

        // Act
        var result = await _classifier.ClassifyAsync(context);

        // Assert
        result.Severity.Should().Be(AlarmSeverity.Info);
        result.SeverityScore.Should().BeLessThan(1.0);
    }

    [Fact]
    public async Task Classify_HighFrequencyCriticalImpact_ReturnsCritical()
    {
        // Arrange
        var context = new AlarmContext
        {
            ErrorFrequency = 95.0,
            SystemImpact = 9.5,
            HistoricalPattern = 8.0
        };

        // Act
        var result = await _classifier.ClassifyAsync(context);

        // Assert
        result.Severity.Should().Be(AlarmSeverity.Critical);
        result.SeverityScore.Should().BeGreaterThan(3.0);
    }

    [Theory]
    [InlineData(5.0, 1.0, 1.0, AlarmSeverity.Info)]
    [InlineData(50.0, 5.0, 5.0, AlarmSeverity.Warning)]
    [InlineData(75.0, 7.0, 7.0, AlarmSeverity.Error)]
    [InlineData(95.0, 9.5, 9.0, AlarmSeverity.Critical)]
    public async Task Classify_VariousInputs_ReturnsExpectedSeverity(
        double errorFreq,
        double impact,
        double pattern,
        AlarmSeverity expectedSeverity)
    {
        // Arrange
        var context = new AlarmContext
        {
            ErrorFrequency = errorFreq,
            SystemImpact = impact,
            HistoricalPattern = pattern
        };

        // Act
        var result = await _classifier.ClassifyAsync(context);

        // Assert
        result.Severity.Should().Be(expectedSeverity);
    }
}
```

### Integration Tests

```csharp
public class AlarmProcessingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AlarmProcessingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessAlarm_WithFuzzyClassification_SavesCorrectSeverity()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new
        {
            Message = "Database connection timeout",
            Type = "DatabaseFailure",
            Source = "ProductionDB"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/alarms", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ProcessedAlarmDto>();
        result.Should().NotBeNull();
        result!.Severity.Should().BeOneOf(AlarmSeverity.Error, AlarmSeverity.Critical);
    }
}
```

---

## ⚡ Performance Optimization

### Caching Rule Evaluations

```csharp
public class CachedFuzzyAlarmClassifier : IFuzzyAlarmClassifier
{
    private readonly IFuzzyAlarmClassifier _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<AlarmClassificationResult> ClassifyAsync(
        AlarmContext context,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(context);

        if (_cache.TryGetValue<AlarmClassificationResult>(cacheKey, out var cached))
        {
            return cached!;
        }

        var result = await _inner.ClassifyAsync(context, cancellationToken);

        _cache.Set(cacheKey, result, _cacheDuration);

        return result;
    }

    private string GetCacheKey(AlarmContext context) =>
        $"alarm_classification_{context.ErrorFrequency:F0}_{context.SystemImpact:F0}_{context.HistoricalPattern:F0}";
}
```

---

## 📊 Monitoring & Observability

### Metrics

```csharp
public class MonitoredFuzzyAlarmClassifier : IFuzzyAlarmClassifier
{
    private readonly IFuzzyAlarmClassifier _inner;
    private readonly ILogger<MonitoredFuzzyAlarmClassifier> _logger;
    private readonly Counter<int> _classificationsCounter;
    private readonly Histogram<double> _classificationDuration;

    public async Task<AlarmClassificationResult> ClassifyAsync(
        AlarmContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await _inner.ClassifyAsync(context, cancellationToken);

            stopwatch.Stop();

            _classificationsCounter.Add(1, new KeyValuePair<string, object?>("severity", result.Severity.ToString()));
            _classificationDuration.Record(stopwatch.ElapsedMilliseconds);

            _logger.LogInformation(
                "Alarm classified: Severity={Severity}, Duration={Duration}ms",
                result.Severity,
                stopwatch.ElapsedMilliseconds
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying alarm");
            throw;
        }
    }
}
```

---

## ✅ Integration Checklist

- [ ] Add project references
- [ ] Define domain interfaces
- [ ] Implement application service
- [ ] Update alarm processing logic
- [ ] Configure dependency injection
- [ ] Add unit tests
- [ ] Add integration tests
- [ ] Implement caching (optional)
- [ ] Add monitoring and metrics
- [ ] Update API documentation
- [ ] Deploy and validate

---

**🎉 Your AlarmInsight application now has intelligent fuzzy logic-based alarm classification!**
