namespace AlarmInsight.Application.Alarms.Queries.GetAlarm;

/// <summary>
/// Data Transfer Object for Alarm entity.
/// Used to return data to API consumers.
/// </summary>
public sealed class AlarmDto
{
    public int Id { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? Resolution { get; init; }
}