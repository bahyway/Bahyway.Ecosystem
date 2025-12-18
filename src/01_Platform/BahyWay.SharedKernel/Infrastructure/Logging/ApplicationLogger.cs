using BahyWay.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace BahyWay.SharedKernel.Infrastructure.Logging;

public class ApplicationLogger<T> : IApplicationLogger<T>
{
    private readonly ILogger<T> _logger;

    public ApplicationLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogInformationWithProperties(string message, Dictionary<string, object> properties)
    {
        using (_logger.BeginScope(properties))
        {
            _logger.LogInformation(message);
        }
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogCritical(Exception exception, string message, params object[] args)
    {
        _logger.LogCritical(exception, message, args);
    }

    public IDisposable BeginScope(Dictionary<string, object> state)
    {
        return _logger.BeginScope(state)!;
    }
}