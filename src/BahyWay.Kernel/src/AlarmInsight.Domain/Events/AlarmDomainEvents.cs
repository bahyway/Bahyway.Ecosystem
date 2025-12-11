using AlarmInsight.Domain.ValueObjects;
using BahyWay.SharedKernel.Domain.Events;

namespace AlarmInsight.Domain.Events;

/// <summary>
/// Domain events for Alarm aggregate.
/// Uses DomainEventBase from SharedKernel.
/// OccurredOn is automatically set by base class.
/// </summary>

public sealed record AlarmCreatedDomainEvent(
    int AlarmId,
    AlarmSeverity Severity,
    Location Location) : DomainEventBase;  // ← REMOVED OccurredOn parameter!

public sealed record AlarmProcessedDomainEvent(
    int AlarmId,
    DateTime ProcessedAt) : DomainEventBase;

public sealed record AlarmResolvedDomainEvent(
    int AlarmId,
    DateTime ResolvedAt,
    string Resolution) : DomainEventBase;

public sealed record AlarmEscalatedDomainEvent(
    int AlarmId,
    AlarmSeverity OldSeverity,
    AlarmSeverity NewSeverity) : DomainEventBase;