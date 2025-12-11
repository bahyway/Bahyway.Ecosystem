using BahyWay.SharedKernel.Domain.Events;

namespace ETLWay.Domain.Events;

/// <summary>
/// Domain event raised when a cemetery plot is created.
/// </summary>
public sealed record PlotCreatedDomainEvent(
    int PlotId,
    string PlotNumber,
    string Section) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a plot is reserved.
/// </summary>
public sealed record PlotReservedDomainEvent(
    int PlotId,
    string PlotNumber,
    DateTime ReservedUntil) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a plot is occupied.
/// </summary>
public sealed record PlotOccupiedDomainEvent(
    int PlotId,
    string PlotNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a plot is released back to available status.
/// </summary>
public sealed record PlotReleasedDomainEvent(
    int PlotId,
    string PlotNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
