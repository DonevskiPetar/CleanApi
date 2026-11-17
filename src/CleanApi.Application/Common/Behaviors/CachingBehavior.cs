using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Domain.Common;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>
/// Serves <see cref="ICachedQuery{TResponse}"/> responses from the cache. Registered per cached query type
/// (see <c>DependencyInjection.AddCachingBehaviors</c>) after validation, so invalid requests never reach the cache.
/// </summary>
internal sealed class CachingBehavior<TQuery, TValue>(ICacheService cache)
    : IPipelineBehavior<TQuery, Result<TValue>>
    where TQuery : ICachedQuery<TValue>
{
    public Task<Result<TValue>> Handle(
        TQuery request, RequestHandlerDelegate<Result<TValue>> next, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            request.CacheKey,
            _ => next(),
            request.CacheTags,
            request.CacheExpiration,
            cancellationToken);
}
