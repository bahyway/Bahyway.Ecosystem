using BahyWay.SharedKernel.Domain.Primitives;

namespace AlarmInsight.Domain;

/// <summary>
/// Domain errors for Alarm aggregate.
/// Uses Error from SharedKernel.
/// PROJECT-SPECIFIC: ✅ AlarmInsight
/// </summary>
public static class AlarmErrors
{
    public static readonly Error SourceRequired =
        Error.Validation("Alarm.SourceRequired", "Alarm source is required");

    public static readonly Error DescriptionRequired =
        Error.Validation("Alarm.DescriptionRequired", "Alarm description is required");

    public static readonly Error SeverityRequired =
        Error.Validation("Alarm.SeverityRequired", "Alarm severity is required");

    public static readonly Error LocationRequired =
        Error.Validation("Alarm.LocationRequired", "Alarm location is required");

    public static readonly Error AlarmNotPending =
        Error.Conflict("Alarm.NotPending", "Only pending alarms can be processed");

    public static readonly Error AlarmAlreadyResolved =
        Error.Conflict("Alarm.AlreadyResolved", "Alarm has already been resolved");

    public static readonly Error ResolutionRequired =
        Error.Validation("Alarm.ResolutionRequired", "Resolution is required to resolve an alarm");

    public static readonly Error NoteContentRequired =
        Error.Validation("Alarm.NoteContentRequired", "Note content is required");

    public static readonly Error NoteAuthorRequired =
        Error.Validation("Alarm.NoteAuthorRequired", "Note author is required");

    public static readonly Error CannotEscalateToLowerSeverity =
        Error.Validation("Alarm.CannotEscalate", "Cannot escalate to lower or same severity");

    public static Error NotFound(int alarmId) =>
        Error.NotFound("Alarm.NotFound", $"Alarm with ID {alarmId} was not found");
}