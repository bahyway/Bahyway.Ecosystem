using BahyWay.SharedKernel.Domain.Entities;
using BahyWay.SharedKernel.Domain.Primitives;
using ETLWay.Domain.Events;
using ETLWay.Domain.ValueObjects;

namespace ETLWay.Domain.Entities;

/// <summary>
/// Represents a cemetery plot or burial site.
/// PROJECT-SPECIFIC: ✅ ETLWay (Cemetery data processing)
/// PATTERN: ✅ Aggregate root with geospatial support
/// USES: GeospatialLocation value object for mapping integration
/// </summary>
public sealed class Plot : SoftDeletableAuditableEntity
{
    private Plot() { } // EF Core

    private Plot(
        string plotNumber,
        string section,
        PlotType type,
        PlotStatus status,
        GeospatialLocation location,
        decimal sizeInSquareMeters)
    {
        PlotNumber = plotNumber;
        Section = section;
        Type = type;
        Status = status;
        Location = location;
        SizeInSquareMeters = sizeInSquareMeters;

        RaiseDomainEvent(new PlotCreatedDomainEvent(Id, plotNumber, section));
    }

    public string PlotNumber { get; private set; } = string.Empty;
    public string Section { get; private set; } = string.Empty;
    public PlotType Type { get; private set; }
    public PlotStatus Status { get; private set; }
    public GeospatialLocation Location { get; private set; } = null!;
    public decimal SizeInSquareMeters { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ReservedUntil { get; private set; }

    /// <summary>
    /// Factory method to create a new cemetery plot.
    /// </summary>
    public static Result<Plot> Create(
        string plotNumber,
        string section,
        PlotType type,
        GeospatialLocation location,
        decimal sizeInSquareMeters)
    {
        if (string.IsNullOrWhiteSpace(plotNumber))
            return Result.Failure<Plot>(PlotErrors.PlotNumberRequired);

        if (string.IsNullOrWhiteSpace(section))
            return Result.Failure<Plot>(PlotErrors.SectionRequired);

        if (sizeInSquareMeters <= 0)
            return Result.Failure<Plot>(PlotErrors.InvalidSize);

        if (location == null)
            return Result.Failure<Plot>(PlotErrors.LocationRequired);

        var plot = new Plot(
            plotNumber,
            section,
            type,
            PlotStatus.Available,
            location,
            sizeInSquareMeters);

        return Result.Success(plot);
    }

    /// <summary>
    /// Reserves the plot for a specific duration.
    /// </summary>
    public Result Reserve(DateTime reservedUntil)
    {
        if (Status != PlotStatus.Available)
            return Result.Failure(PlotErrors.PlotNotAvailable);

        if (reservedUntil <= DateTime.UtcNow)
            return Result.Failure(PlotErrors.InvalidReservationDate);

        Status = PlotStatus.Reserved;
        ReservedUntil = reservedUntil;

        RaiseDomainEvent(new PlotReservedDomainEvent(Id, PlotNumber, reservedUntil));
        return Result.Success();
    }

    /// <summary>
    /// Marks the plot as occupied.
    /// </summary>
    public Result Occupy()
    {
        if (Status == PlotStatus.Occupied)
            return Result.Failure(PlotErrors.PlotAlreadyOccupied);

        Status = PlotStatus.Occupied;
        ReservedUntil = null;

        RaiseDomainEvent(new PlotOccupiedDomainEvent(Id, PlotNumber));
        return Result.Success();
    }

    /// <summary>
    /// Releases the plot back to available status.
    /// </summary>
    public Result Release()
    {
        if (Status == PlotStatus.Available)
            return Result.Failure(PlotErrors.PlotAlreadyAvailable);

        Status = PlotStatus.Available;
        ReservedUntil = null;

        RaiseDomainEvent(new PlotReleasedDomainEvent(Id, PlotNumber));
        return Result.Success();
    }

    /// <summary>
    /// Updates plot notes.
    /// </summary>
    public Result UpdateNotes(string notes)
    {
        Notes = notes;
        return Result.Success();
    }
}

/// <summary>
/// Plot type enumeration.
/// </summary>
public enum PlotType
{
    SingleGrave = 1,
    FamilyPlot = 2,
    ChildrenGrave = 3,
    MausoleumCrypt = 4
}

/// <summary>
/// Plot status enumeration.
/// </summary>
public enum PlotStatus
{
    Available = 1,
    Reserved = 2,
    Occupied = 3,
    Maintenance = 4
}

/// <summary>
/// Domain errors for Plot entity.
/// </summary>
public static class PlotErrors
{
    public static readonly Error PlotNumberRequired = new("Plot.PlotNumberRequired", "Plot number is required");
    public static readonly Error SectionRequired = new("Plot.SectionRequired", "Section is required");
    public static readonly Error LocationRequired = new("Plot.LocationRequired", "Location is required");
    public static readonly Error InvalidSize = new("Plot.InvalidSize", "Size must be greater than zero");
    public static readonly Error PlotNotAvailable = new("Plot.NotAvailable", "Plot is not available for reservation");
    public static readonly Error PlotAlreadyOccupied = new("Plot.AlreadyOccupied", "Plot is already occupied");
    public static readonly Error PlotAlreadyAvailable = new("Plot.AlreadyAvailable", "Plot is already available");
    public static readonly Error InvalidReservationDate = new("Plot.InvalidReservationDate", "Reservation date must be in the future");
}
