using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Domain.Common;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>
/// After an <see cref="ICacheInvalidatingCommand"/> succeeds, evicts the cache tags it declares.
/// Failed results leave the cache untouched because nothing was changed.
/// </summary>
internal sealed class CacheInvalidationBehavior<TRequest, TResponse>(ICacheService cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICacheInvalidatingCommand command && response is Result { IsSuccess: true })
        {
            // The change is already committed, so a caller cancellation must not skip the eviction.
            await cache.RemoveByTagsAsync(command.CacheTagsToInvalidate, CancellationToken.None);
        }

        return response;
    }
}
