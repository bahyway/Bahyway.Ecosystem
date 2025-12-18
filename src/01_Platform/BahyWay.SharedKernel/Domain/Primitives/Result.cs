using System;

namespace BahyWay.SharedKernel.Domain.Primitives;

/// <summary>
/// Represents the result of an operation.
/// REUSABLE: ✅ ALL PROJECTS
/// PATTERN: Railway-oriented programming (no exceptions for business failures)
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error that occurred (if any).
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failure result with an error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a success result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failure result with an error (no value).
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    /// <summary>
    /// Creates a result based on a condition.
    /// </summary>
    public static Result Create(bool condition, Error error) =>
        condition ? Success() : Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// REUSABLE: ✅ ALL PROJECTS
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the result value.
    /// Throws if the result is a failure.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result");

    /// <summary>
    /// Implicitly converts a value to a success result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) =>
        Success(value);
}