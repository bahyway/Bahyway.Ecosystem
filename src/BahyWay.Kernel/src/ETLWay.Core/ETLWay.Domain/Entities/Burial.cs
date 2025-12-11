using BahyWay.SharedKernel.Domain.Entities;
using BahyWay.SharedKernel.Domain.Primitives;
using ETLWay.Domain.Events;
using ETLWay.Domain.ValueObjects;

namespace ETLWay.Domain.Entities;

/// <summary>
/// Represents a burial record linking a deceased person to a plot.
/// PROJECT-SPECIFIC: ✅ ETLWay (Cemetery data processing)
/// PATTERN: ✅ Aggregate root managing burial lifecycle
/// AGGREGATE: Links Deceased entity with Plot entity
/// </summary>
public sealed class Burial : SoftDeletableAuditableEntity
{
    private readonly List<BurialDocument> _documents = new();

    private Burial() { } // EF Core

    private Burial(
        int deceasedId,
        int plotId,
        DateTime burialDate,
        string burialPermitNumber,
        string registeredBy)
    {
        DeceasedId = deceasedId;
        PlotId = plotId;
        BurialDate = burialDate;
        BurialPermitNumber = burialPermitNumber;
        RegisteredBy = registeredBy;
        Status = BurialStatus.Pending;

        RaiseDomainEvent(new BurialCreatedDomainEvent(Id, deceasedId, plotId, burialDate));
    }

    public int DeceasedId { get; private set; }
    public int PlotId { get; private set; }
    public DateTime BurialDate { get; private set; }
    public string BurialPermitNumber { get; private set; } = string.Empty;
    public string RegisteredBy { get; private set; } = string.Empty;
    public BurialStatus Status { get; private set; }
    public string? CauseOfDeath { get; private set; }
    public string? Notes { get; private set; }
    public string? CeremonyDetails { get; private set; }

    // Navigation properties
    public Deceased? Deceased { get; private set; }
    public Plot? Plot { get; private set; }
    public IReadOnlyCollection<BurialDocument> Documents => _documents.AsReadOnly();

    /// <summary>
    /// Factory method to create a new burial record.
    /// </summary>
    public static Result<Burial> Create(
        int deceasedId,
        int plotId,
        DateTime burialDate,
        string burialPermitNumber,
        string registeredBy)
    {
        if (deceasedId <= 0)
            return Result.Failure<Burial>(BurialErrors.InvalidDeceasedId);

        if (plotId <= 0)
            return Result.Failure<Burial>(BurialErrors.InvalidPlotId);

        if (string.IsNullOrWhiteSpace(burialPermitNumber))
            return Result.Failure<Burial>(BurialErrors.PermitNumberRequired);

        if (string.IsNullOrWhiteSpace(registeredBy))
            return Result.Failure<Burial>(BurialErrors.RegisteredByRequired);

        if (burialDate > DateTime.UtcNow.AddDays(30))
            return Result.Failure<Burial>(BurialErrors.InvalidBurialDate);

        var burial = new Burial(
            deceasedId,
            plotId,
            burialDate,
            burialPermitNumber,
            registeredBy);

        return Result.Success(burial);
    }

    /// <summary>
    /// Confirms the burial has been completed.
    /// </summary>
    public Result Confirm()
    {
        if (Status != BurialStatus.Pending)
            return Result.Failure(BurialErrors.BurialNotPending);

        Status = BurialStatus.Completed;
        RaiseDomainEvent(new BurialConfirmedDomainEvent(Id, BurialDate));

        return Result.Success();
    }

    /// <summary>
    /// Cancels a pending burial.
    /// </summary>
    public Result Cancel(string reason)
    {
        if (Status == BurialStatus.Completed)
            return Result.Failure(BurialErrors.CannotCancelCompleted);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(BurialErrors.CancellationReasonRequired);

        Status = BurialStatus.Cancelled;
        Notes = $"Cancelled: {reason}";

        RaiseDomainEvent(new BurialCancelledDomainEvent(Id, reason));
        return Result.Success();
    }

    /// <summary>
    /// Adds a document to the burial record.
    /// </summary>
    public Result AddDocument(string documentType, string documentUrl, string uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            return Result.Failure(BurialErrors.DocumentTypeRequired);

        if (string.IsNullOrWhiteSpace(documentUrl))
            return Result.Failure(BurialErrors.DocumentUrlRequired);

        var document = new BurialDocument
        {
            BurialId = Id,
            DocumentType = documentType,
            DocumentUrl = documentUrl,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        _documents.Add(document);
        RaiseDomainEvent(new BurialDocumentAddedDomainEvent(Id, documentType));

        return Result.Success();
    }

    /// <summary>
    /// Updates burial details.
    /// </summary>
    public Result UpdateDetails(
        string? causeOfDeath = null,
        string? notes = null,
        string? ceremonyDetails = null)
    {
        if (causeOfDeath != null) CauseOfDeath = causeOfDeath;
        if (notes != null) Notes = notes;
        if (ceremonyDetails != null) CeremonyDetails = ceremonyDetails;

        return Result.Success();
    }
}

/// <summary>
/// Represents a document associated with a burial.
/// </summary>
public class BurialDocument
{
    public int Id { get; set; }
    public int BurialId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// Burial status enumeration.
/// </summary>
public enum BurialStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>
/// Domain errors for Burial entity.
/// </summary>
public static class BurialErrors
{
    public static readonly Error InvalidDeceasedId = new("Burial.InvalidDeceasedId", "Deceased ID must be greater than zero");
    public static readonly Error InvalidPlotId = new("Burial.InvalidPlotId", "Plot ID must be greater than zero");
    public static readonly Error PermitNumberRequired = new("Burial.PermitNumberRequired", "Burial permit number is required");
    public static readonly Error RegisteredByRequired = new("Burial.RegisteredByRequired", "Registered by is required");
    public static readonly Error InvalidBurialDate = new("Burial.InvalidBurialDate", "Burial date cannot be more than 30 days in the future");
    public static readonly Error BurialNotPending = new("Burial.NotPending", "Burial must be in pending status");
    public static readonly Error CannotCancelCompleted = new("Burial.CannotCancelCompleted", "Cannot cancel a completed burial");
    public static readonly Error CancellationReasonRequired = new("Burial.CancellationReasonRequired", "Cancellation reason is required");
    public static readonly Error DocumentTypeRequired = new("Burial.DocumentTypeRequired", "Document type is required");
    public static readonly Error DocumentUrlRequired = new("Burial.DocumentUrlRequired", "Document URL is required");
}
