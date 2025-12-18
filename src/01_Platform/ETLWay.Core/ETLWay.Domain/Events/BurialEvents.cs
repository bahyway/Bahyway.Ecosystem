using BahyWay.SharedKernel.Domain.Events;

namespace ETLWay.Domain.Events;

/// <summary>
/// Domain event raised when a burial record is created.
/// </summary>
public sealed record BurialCreatedDomainEvent(
    int BurialId,
    int DeceasedId,
    int PlotId,
    DateTime BurialDate) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a burial is confirmed as completed.
/// </summary>
public sealed record BurialConfirmedDomainEvent(
    int BurialId,
    DateTime BurialDate) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a burial is cancelled.
/// </summary>
public sealed record BurialCancelledDomainEvent(
    int BurialId,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Domain event raised when a document is added to a burial record.
/// </summary>
public sealed record BurialDocumentAddedDomainEvent(
    int BurialId,
    string DocumentType) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
