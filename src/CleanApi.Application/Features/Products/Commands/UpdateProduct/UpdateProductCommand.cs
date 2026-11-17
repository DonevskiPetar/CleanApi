using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price)
    : ICommand<ProductResponse>, ICacheInvalidatingCommand
{
    IReadOnlyCollection<string> ICacheInvalidatingCommand.CacheTagsToInvalidate =>
        [ProductCacheKeys.ById(Id), ProductCacheKeys.ListTag];
}
