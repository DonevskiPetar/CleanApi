using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Abstractions.Caching;

/// <summary>
/// Opt-in caching for a query. Implement <see cref="ICachedQuery{TResponse}"/> instead of <see cref="IQuery{TResponse}"/>
/// and <c>CachingBehavior</c> serves the response from the cache. Use explicit interface implementation
/// so these members don't become part of the record's public shape.
/// </summary>
public interface ICachedQuery
{
    /// <summary>Unique key for this query and its parameters, e.g. <c>products:{id}</c>.</summary>
    string CacheKey { get; }

    /// <summary>Tags used to invalidate the entry; commands evict by tag (see <see cref="ICacheInvalidatingCommand"/>).</summary>
    IReadOnlyCollection<string> CacheTags { get; }

    /// <summary>Time to live; <see langword="null"/> uses the configured default (<c>Caching:DefaultExpiration</c>).</summary>
    TimeSpan? CacheExpiration => null;
}

/// <inheritdoc cref="ICachedQuery"/>
public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery;
