using BahyWay.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace BahyWay.SharedKernel.Infrastructure.Caching;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public InMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateAsync(key, async entry =>
        {
            if (expiration.HasValue)
                entry.SetAbsoluteExpiration(expiration.Value);
            lock (_lock) { _keys.Add(key); }
            return await factory();
        });
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
        _cache.Set(key, value, options);
        lock (_lock) { _keys.Add(key); }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        lock (_lock) { _keys.Remove(key); }
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var regex = new Regex(pattern, RegexOptions.Compiled);
        List<string> keysToRemove;
        lock (_lock) { keysToRemove = _keys.Where(k => regex.IsMatch(k)).ToList(); }
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            lock (_lock) { _keys.Remove(key); }
        }
        return Task.CompletedTask;
    }
}