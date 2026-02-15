using FB_App.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;

namespace FB_App.Infrastructure.Caching;

public sealed class CacheService : ICacheService
{
    private readonly HybridCache _cache;

    public CacheService(HybridCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = expiration.HasValue
            ? new HybridCacheEntryOptions
            {
                Expiration = expiration.Value,
                LocalCacheExpiration = TimeSpan.FromSeconds(expiration.Value.TotalSeconds / 2)
            }
            : null;

        return await _cache.GetOrCreateAsync(
            key,
            async (ct) => await factory(ct),
            options,
            cancellationToken: cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
