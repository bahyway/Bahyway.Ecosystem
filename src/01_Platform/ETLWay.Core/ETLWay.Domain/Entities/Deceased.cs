using BahyWay.SharedKernel.Domain.Entities;
using BahyWay.SharedKernel.Domain.Primitives;
using ETLWay.Domain.Events;
using ETLWay.Domain.ValueObjects;

namespace ETLWay.Domain.Entities;

/// <summary>
/// Represents a deceased person in the cemetery management system.
/// PROJECT-SPECIFIC: ✅ ETLWay (Cemetery data processing)
/// PATTERN: ✅ Aggregate root pattern - reusable across cemetery projects
/// CRITICAL FOR: NajafCemetery (legal records), ETLway (data lineage)
/// </summary>
public sealed class Deceased : SoftDeletableAuditableEntity
{
    private Deceased() { } // EF Core

    private Deceased(
        string fullName,
        string arabicName,
        DateTime dateOfBirth,
        DateTime dateOfDeath,
        string nationalId,
        string? fatherName,
        string? motherName,
        string? nationality,
        string? religion)
    {
        FullName = fullName;
        ArabicName = arabicName;
        DateOfBirth = dateOfBirth;
        DateOfDeath = dateOfDeath;
        NationalId = nationalId;
        FatherName = fatherName;
        MotherName = motherName;
        Nationality = nationality;
        Religion = religion;

        RaiseDomainEvent(new DeceasedCreatedDomainEvent(Id, FullName, DateOfDeath));
    }

    public string FullName { get; private set; } = string.Empty;
    public string ArabicName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public DateTime DateOfDeath { get; private set; }
    public string NationalId { get; private set; } = string.Empty;
    public string? FatherName { get; private set; }
    public string? MotherName { get; private set; }
    public string? Nationality { get; private set; }
    public string? Religion { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Factory method to create a new deceased person record.
    /// </summary>
    public static Result<Deceased> Create(
        string fullName,
        string arabicName,
        DateTime dateOfBirth,
        DateTime dateOfDeath,
        string nationalId,
        string? fatherName = null,
        string? motherName = null,
        string? nationality = null,
        string? religion = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<Deceased>(DeceasedErrors.FullNameRequired);

        if (string.IsNullOrWhiteSpace(arabicName))
            return Result.Failure<Deceased>(DeceasedErrors.ArabicNameRequired);

        if (string.IsNullOrWhiteSpace(nationalId))
            return Result.Failure<Deceased>(DeceasedErrors.NationalIdRequired);

        if (dateOfDeath < dateOfBirth)
            return Result.Failure<Deceased>(DeceasedErrors.InvalidDateOfDeath);

        if (dateOfDeath > DateTime.UtcNow)
            return Result.Failure<Deceased>(DeceasedErrors.FutureDateNotAllowed);

        var deceased = new Deceased(
            fullName,
            arabicName,
            dateOfBirth,
            dateOfDeath,
            nationalId,
            fatherName,
            motherName,
            nationality,
            religion);

        return Result.Success(deceased);
    }

    /// <summary>
    /// Updates the photo URL for the deceased person.
    /// </summary>
    public Result UpdatePhoto(string photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            return Result.Failure(DeceasedErrors.PhotoUrlRequired);

        PhotoUrl = photoUrl;
        RaiseDomainEvent(new DeceasedPhotoUpdatedDomainEvent(Id, photoUrl));

        return Result.Success();
    }

    /// <summary>
    /// Adds or updates notes for the deceased person.
    /// </summary>
    public Result UpdateNotes(string notes)
    {
        Notes = notes;
        return Result.Success();
    }

    /// <summary>
    /// Updates basic information of the deceased person.
    /// </summary>
    public Result Update(
        string? fatherName = null,
        string? motherName = null,
        string? nationality = null,
        string? religion = null)
    {
        if (fatherName != null) FatherName = fatherName;
        if (motherName != null) MotherName = motherName;
        if (nationality != null) Nationality = nationality;
        if (religion != null) Religion = religion;

        return Result.Success();
    }
}

/// <summary>
/// Domain errors for Deceased entity.
/// </summary>
public static class DeceasedErrors
{
    public static readonly Error FullNameRequired = new("Deceased.FullNameRequired", "Full name is required");
    public static readonly Error ArabicNameRequired = new("Deceased.ArabicNameRequired", "Arabic name is required");
    public static readonly Error NationalIdRequired = new("Deceased.NationalIdRequired", "National ID is required");
    public static readonly Error InvalidDateOfDeath = new("Deceased.InvalidDateOfDeath", "Date of death cannot be before date of birth");
    public static readonly Error FutureDateNotAllowed = new("Deceased.FutureDateNotAllowed", "Date of death cannot be in the future");
    public static readonly Error PhotoUrlRequired = new("Deceased.PhotoUrlRequired", "Photo URL is required");
}
