using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Common.Models;

namespace CleanApi.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : ICachedQuery<PagedResult<ProductResponse>>
{
    string ICachedQuery.CacheKey => ProductCacheKeys.List(Page, PageSize, Search);

    IReadOnlyCollection<string> ICachedQuery.CacheTags => [ProductCacheKeys.ListTag];
}
