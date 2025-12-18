namespace BahyWay.SharedKernel.Application.Abstractions;

/// <summary>
/// Application-level logging abstraction.
/// Provides structured logging with correlation ID support.
/// REUSABLE: ✅ ALL PROJECTS
/// </summary>
public interface IApplicationLogger<T>
{
    /// <summary>
    /// Logs debug information.
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs informational messages.
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs warning messages.
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs error with exception details.
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs critical errors requiring immediate attention.
    /// </summary>
    void LogCritical(Exception exception, string message, params object[] args);

    /// <summary>
    /// Creates a logging scope with additional properties.
    /// </summary>
    IDisposable BeginScope(Dictionary<string, object> properties);

    /// <summary>
    /// Logs with custom structured properties.
    /// </summary>
    void LogInformationWithProperties(string message, Dictionary<string, object> properties);
}