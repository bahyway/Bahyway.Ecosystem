using System;
using System.Collections.Generic;
using BahyWay.SharedKernel.Domain.Primitives;

namespace AlarmInsight.Domain.ValueObjects;

/// <summary>
/// Represents alarm severity as a value object (more flexible than enum).
/// PROJECT-SPECIFIC: ✅ AlarmInsight
/// ADVANTAGES: Can add validation, business logic, database compatibility
/// </summary>
public sealed class AlarmSeverity : ValueObject
{
    public static readonly AlarmSeverity Low = new(1, "Low");
    public static readonly AlarmSeverity Medium = new(2, "Medium");
    public static readonly AlarmSeverity High = new(3, "High");
    public static readonly AlarmSeverity Critical = new(4, "Critical");

    private static readonly Dictionary<int, AlarmSeverity> _byValue = new()
    {
        { 1, Low },
        { 2, Medium },
        { 3, High },
        { 4, Critical }
    };

    private AlarmSeverity(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; }
    public string Name { get; }

    /// <summary>
    /// Creates severity from integer value.
    /// </summary>
    public static Result<AlarmSeverity> FromValue(int value)
    {
        if (_byValue.TryGetValue(value, out var severity))
            return Result.Success(severity);

        return Result.Failure<AlarmSeverity>(
            Error.Validation("AlarmSeverity.Invalid", $"Invalid severity value: {value}"));
    }

    /// <summary>
    /// Creates severity from name.
    /// </summary>
    public static Result<AlarmSeverity> FromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<AlarmSeverity>(
                Error.Validation("AlarmSeverity.Empty", "Severity name cannot be empty"));

        return name.ToLowerInvariant() switch
        {
            "low" => Result.Success(Low),
            "medium" => Result.Success(Medium),
            "high" => Result.Success(High),
            "critical" => Result.Success(Critical),
            _ => Result.Failure<AlarmSeverity>(
                Error.Validation("AlarmSeverity.Invalid", $"Invalid severity: {name}"))
        };
    }

    /// <summary>
    /// Checks if this severity is higher than another.
    /// </summary>
    public bool IsHigherThan(AlarmSeverity other) => Value > other.Value;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Name;
}