using BahyWay.SharedKernel.Domain.Primitives;

namespace ETLWay.Application.Abstractions.Services;

/// <summary>
/// Service interface for cemetery operations.
/// PROJECT-SPECIFIC: ✅ ETLWay
/// PATTERN: ✅ Application Service pattern
/// PURPOSE: Orchestrates cemetery operations across multiple aggregates
/// </summary>
public interface ICemeteryService
{
    /// <summary>
    /// Registers a complete burial (deceased + plot + burial record).
    /// </summary>
    Task<Result<int>> RegisterBurialAsync(
        RegisterBurialRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a plot is available for burial.
    /// </summary>
    Task<Result> ValidatePlotAvailabilityAsync(
        int plotId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfers a burial from one plot to another (exhumation/reburial).
    /// </summary>
    Task<Result> TransferBurialAsync(
        int burialId,
        int newPlotId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a burial certificate.
    /// </summary>
    Task<Result<byte[]>> GenerateBurialCertificateAsync(
        int burialId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for registering a complete burial.
/// </summary>
public sealed record RegisterBurialRequest(
    // Deceased information
    string FullName,
    string ArabicName,
    DateTime DateOfBirth,
    DateTime DateOfDeath,
    string NationalId,
    string? FatherName,
    string? MotherName,
    string? Nationality,
    string? Religion,
    // Burial information
    int PlotId,
    DateTime BurialDate,
    string BurialPermitNumber,
    string RegisteredBy,
    string? CauseOfDeath,
    string? CeremonyDetails
);
