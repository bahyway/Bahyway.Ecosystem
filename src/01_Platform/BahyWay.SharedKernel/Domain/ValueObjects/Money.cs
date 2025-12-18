using System;
using System.Collections.Generic;
using BahyWay.SharedKernel.Domain.Primitives;

namespace BahyWay.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// REUSABLE: ✅ HireWay (salaries), any project with financial data
/// EXAMPLE: Shows how to create complex value objects
/// </summary>
public sealed class Money : ValueObject
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return Result.Failure<Money>(
                Error.Validation("Money.NegativeAmount", "Amount cannot be negative"));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>(
                Error.Validation("Money.InvalidCurrency", "Currency is required"));

        if (currency.Length != 3)
            return Result.Failure<Money>(
                Error.Validation("Money.InvalidCurrency", "Currency must be 3-letter ISO code"));

        return Result.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    /// <summary>
    /// Adds two money values (must have same currency).
    /// </summary>
    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return Result.Failure<Money>(
                Error.Validation("Money.CurrencyMismatch", "Cannot add different currencies"));

        return Create(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";

}