namespace BahyWay.SharedKernel.Application.Abstractions;

/// <summary>
/// Caching service abstraction supporting both in-memory and distributed caching.
/// REUSABLE: ✅ ALL PROJECTS
/// HEAVY USERS: SmartForesight (forecast results), SteerView (map data), AlarmInsight (active alarms)
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets cached value or creates it using factory function.
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached value.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a cached value.
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values matching pattern (e.g., "alarm:*").
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Helper class for building consistent cache keys.
/// </summary>
public static class CacheKeys
{
    private const string Delimiter = ":";

    public static string BuildKey(params object[] parts) =>
        string.Join(Delimiter, parts.Select(p => p?.ToString() ?? "null"));

    public static string BuildPattern(params object[] parts) => BuildKey(parts);

    // AlarmInsight specific keys
    public static class Alarms
    {
        public static string ById(int id) => BuildKey("alarm", id);
        public static string ByLocation(string location) => BuildKey("alarm", "location", location);
        public static string AllActive() => BuildKey("alarm", "active");
        public static string Pattern() => BuildPattern("alarm", "*");
    }
}

/// <summary>
/// Standard cache expiration times.
/// </summary>
public static class CacheExpiration
{
    public static readonly TimeSpan VeryShort = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Medium = TimeSpan.FromHours(1);
    public static readonly TimeSpan Long = TimeSpan.FromHours(4);
    public static readonly TimeSpan VeryLong = TimeSpan.FromDays(1);
}
