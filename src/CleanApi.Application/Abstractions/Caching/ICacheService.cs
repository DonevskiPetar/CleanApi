using CleanApi.Domain.Common;

namespace CleanApi.Application.Abstractions.Caching;

/// <summary>
/// Application-facing cache. Implemented in Infrastructure (HybridCache: in-memory L1 + Redis L2),
/// so handlers and behaviors never depend on a concrete cache technology.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or runs <paramref name="factory"/> and caches its value.
    /// Only successful results are cached; a failed <see cref="Result"/> is returned as-is and not stored.
    /// Concurrent callers for the same key share a single factory call (stampede protection).
    /// </summary>
    Task<Result<T>> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        IReadOnlyCollection<string> tags,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>Evicts every entry that was stored with any of the given tags.</summary>
    Task RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
}
