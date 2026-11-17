namespace CleanApi.Application.Abstractions.Caching;

/// <summary>
/// A command that makes cached query results stale. After the handler returns a successful result,
/// <c>CacheInvalidationBehavior</c> evicts every entry tagged with <see cref="CacheTagsToInvalidate"/>.
/// </summary>
public interface ICacheInvalidatingCommand
{
    IReadOnlyCollection<string> CacheTagsToInvalidate { get; }
}
