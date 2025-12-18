using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BahyWay.SharedKernel.Domain.Primitives;

namespace BahyWay.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a phone number value object.
/// REUSABLE: ✅ HireWay (contact info), NajafCemetery (family contact), AlarmInsight (notification)
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$", // E.164 format
        RegexOptions.Compiled);

    private PhoneNumber(string value, string? countryCode = null)
    {
        Value = value;
        CountryCode = countryCode ?? "+1"; // Default to US
    }

    public string Value { get; }
    public string CountryCode { get; }

    /// <summary>
    /// Full international format: +1-555-0100
    /// </summary>
    public string FullNumber => $"{CountryCode}{Value}";

    public static Result<PhoneNumber> Create(string? phoneNumber, string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.Empty", "Phone number cannot be empty"));

        // Remove common formatting characters
        var cleaned = Regex.Replace(phoneNumber, @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleaned))
            return Result.Failure<PhoneNumber>(
                Error.Validation("PhoneNumber.Invalid", "Phone number format is invalid"));

        return Result.Success(new PhoneNumber(cleaned, countryCode));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
        yield return CountryCode;
    }

    public override string ToString() => FullNumber;
}