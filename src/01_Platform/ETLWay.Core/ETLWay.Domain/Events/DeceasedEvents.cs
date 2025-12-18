using BahyWay.SharedKernel.Domain.Events;

namespace ETLWay.Domain.Events;

/// <summary>
/// Domain event raised when a deceased person record is created.
/// </summary>
public sealed record DeceasedCreatedDomainEvent(
    int DeceasedId,
    string FullName,
    DateTime DateOfDeath) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a deceased person's photo is updated.
/// </summary>
public sealed record DeceasedPhotoUpdatedDomainEvent(
    int DeceasedId,
    string PhotoUrl) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
