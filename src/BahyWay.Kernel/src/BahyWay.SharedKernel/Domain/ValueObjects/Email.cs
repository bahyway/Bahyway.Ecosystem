using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BahyWay.SharedKernel.Domain.Primitives;

namespace BahyWay.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents an email address value object.
/// REUSABLE: ✅ HireWay, AlarmInsight (notifications)
/// EXAMPLE: Shows how to create value objects
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Creates an email from a string.
    /// </summary>
    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(
                Error.Validation("Email.Empty", "Email cannot be empty"));

        if (!EmailRegex.IsMatch(email))
            return Result.Failure<Email>(
                Error.Validation("Email.Invalid", "Email format is invalid"));

        return Result.Success(new Email(email.ToLowerInvariant()));
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

}