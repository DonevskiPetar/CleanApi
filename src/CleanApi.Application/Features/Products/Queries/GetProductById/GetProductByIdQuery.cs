using CleanApi.Application.Abstractions.Caching;

namespace CleanApi.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : ICachedQuery<ProductResponse>
{
    string ICachedQuery.CacheKey => ProductCacheKeys.ById(Id);

    IReadOnlyCollection<string> ICachedQuery.CacheTags => [ProductCacheKeys.ById(Id)];
}
