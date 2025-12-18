namespace BahyWay.SharedKernel.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain.
/// REUSABLE: ✅ ALL PROJECTS
/// PATTERN: Domain-Driven Design Event Sourcing
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    DateTime OccurredOn { get; }
}