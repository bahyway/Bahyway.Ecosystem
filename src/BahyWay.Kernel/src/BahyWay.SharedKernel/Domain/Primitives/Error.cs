namespace BahyWay.SharedKernel.Domain.Primitives;

/// <summary>
/// Represents an error with a code and message.
/// REUSABLE: ✅ ALL PROJECTS
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Represents no error (success case).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new(
        "Error.NullValue",
        "A null value was provided");

    //private Error(string code, string message)
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// The error code (e.g., "Alarm.NotFound").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a new error.
    /// </summary>
    public static Error Create(string code, string message) =>
        new(code, message);

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(string code, string message) =>
        new($"Validation.{code}", message);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static Error NotFound(string code, string message) =>
        new($"NotFound.{code}", message);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static Error Conflict(string code, string message) =>
        new($"Conflict.{code}", message);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(string code, string message) =>
        new($"Unauthorized.{code}", message);
}