namespace FB_App.Application.Common.Interfaces;

/// <summary>
/// Abstraction for hybrid caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets or creates a cached value.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache key constants for the application.
/// </summary>
public static class CacheKeys
{
    public static string MoviesList(int page, int size, string? search, string? genre) 
        => $"movies:list:{page}:{size}:{search ?? "all"}:{genre ?? "all"}";

    public static string MovieById(Guid id) => $"movies:{id}";

    public static string CommentsByMovie(Guid movieId) => $"comments:movie:{movieId}";
}
