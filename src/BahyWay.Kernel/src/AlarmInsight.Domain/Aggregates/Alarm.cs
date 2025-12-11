using AlarmInsight.Domain.Events;
using AlarmInsight.Domain.ValueObjects;
using BahyWay.SharedKernel.Domain.Entities;
using BahyWay.SharedKernel.Domain.Primitives;

namespace AlarmInsight.Domain.Aggregates;

/// <summary>
/// Alarm aggregate root.
/// Represents an alarm in the system with its complete lifecycle.
/// PROJECT-SPECIFIC: ✅ AlarmInsight
/// PATTERN: ✅ DDD Aggregate Root (uses Entity, Result, Value Objects from SharedKernel)
/// </summary>
public sealed class Alarm : AuditableEntity
{
    private readonly List<AlarmNote> _notes = new();

    // Private constructor for EF Core
    private Alarm() { }

    // Private constructor for factory method
    private Alarm(
        string source,
        string description,
        AlarmSeverity alarmSeverity,
        Location alarmLocation)
    {
        Source = source;
        Description = description;
        Severity = alarmSeverity;
        Location = alarmLocation;
        Status = AlarmStatus.Pending;
        OccurredAt = DateTime.UtcNow;

        // Raise domain event (OccurredOn automatically set by DomainEventBase)
        RaiseDomainEvent(new AlarmCreatedDomainEvent(Id, alarmSeverity, alarmLocation));
    }

    // Properties (using value objects from SharedKernel and AlarmInsight)
    public string Source { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AlarmSeverity Severity { get; private set; } = AlarmSeverity.Low;
    public Location Location { get; private set; } = null!;
    public AlarmStatus Status { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? Resolution { get; private set; }

    // Navigation properties
    public IReadOnlyCollection<AlarmNote> Notes => _notes.AsReadOnly();

    /// <summary>
    /// Factory method to create a new alarm.
    /// Uses Result pattern from SharedKernel for validation.
    /// </summary>
    public static Result<Alarm> Create(
        string? source,
        string? description,
        AlarmSeverity? severity,
        Location? location)
    {
        // Validation using Result pattern (from SharedKernel)
        if (string.IsNullOrWhiteSpace(source))
            return Result.Failure<Alarm>(AlarmErrors.SourceRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Alarm>(AlarmErrors.DescriptionRequired);

        if (severity is null)
            return Result.Failure<Alarm>(AlarmErrors.SeverityRequired);

        if (location is null)
            return Result.Failure<Alarm>(AlarmErrors.LocationRequired);

        var alarm = new Alarm(source, description, severity, location);
        return Result.Success(alarm);
    }

    /// <summary>
    /// Process the alarm (business logic).
    /// </summary>
    public Result Process()
    {
        if (Status != AlarmStatus.Pending)
            return Result.Failure(AlarmErrors.AlarmNotPending);

        Status = AlarmStatus.Processing;
        ProcessedAt = DateTime.UtcNow;

        // Raise domain event
        RaiseDomainEvent(new AlarmProcessedDomainEvent(Id, ProcessedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Resolve the alarm with a resolution note.
    /// </summary>
    public Result Resolve(string? resolution)
    {
        if (Status == AlarmStatus.Resolved)
            return Result.Failure(AlarmErrors.AlarmAlreadyResolved);

        if (string.IsNullOrWhiteSpace(resolution))
            return Result.Failure(AlarmErrors.ResolutionRequired);

        Status = AlarmStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        Resolution = resolution;

        // Raise domain event
        RaiseDomainEvent(new AlarmResolvedDomainEvent(Id, ResolvedAt.Value, resolution));

        return Result.Success();
    }

    /// <summary>
    /// Add a note to the alarm.
    /// </summary>
    public Result<AlarmNote> AddNote(string? content, string? author)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<AlarmNote>(AlarmErrors.NoteContentRequired);

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure<AlarmNote>(AlarmErrors.NoteAuthorRequired);

        var note = new AlarmNote(content, author);
        _notes.Add(note);

        return Result.Success(note);
    }

    /// <summary>
    /// Escalate alarm to higher severity.
    /// </summary>
    public Result Escalate(AlarmSeverity newSeverity)
    {
        if (!newSeverity.IsHigherThan(Severity))
            return Result.Failure(AlarmErrors.CannotEscalateToLowerSeverity);

        var oldSeverity = Severity;
        Severity = newSeverity;

        // Raise domain event
        RaiseDomainEvent(new AlarmEscalatedDomainEvent(Id, oldSeverity, newSeverity));

        return Result.Success();
    }
}

/// <summary>
/// Alarm status enum (simple enum is fine here, no complex validation needed).
/// </summary>
public enum AlarmStatus
{
    Pending = 1,
    Processing = 2,
    Resolved = 3,
    Cancelled = 4
}

/// <summary>
/// Alarm note entity (child entity of Alarm aggregate).
/// </summary>
public class AlarmNote : Entity
{
    private AlarmNote() { } // EF Core

    internal AlarmNote(string content, string author)
    {
        Content = content;
        Author = author;
        CreatedAt = DateTime.UtcNow;
    }

    public string Content { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
}