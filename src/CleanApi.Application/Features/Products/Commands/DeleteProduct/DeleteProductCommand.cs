using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyCollection<string> ICacheInvalidatingCommand.CacheTagsToInvalidate =>
        [ProductCacheKeys.ById(Id), ProductCacheKeys.ListTag];
}
