namespace AlarmInsight.Application.Alarms.Queries.GetActiveAlarms;

/// <summary>
/// Summary DTO for alarm lists (lighter than full AlarmDto).
/// </summary>
public sealed class AlarmSummaryDto
{
    public int Id { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}