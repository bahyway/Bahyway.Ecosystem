using System;

namespace BahyWay.SharedKernel.Domain.Events;

/// <summary>
/// Base class for domain events with common properties.
/// REUSABLE: ✅ ALL PROJECTS
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    protected DomainEventBase()
    {
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }

    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    public DateTime OccurredOn { get; init; }

}