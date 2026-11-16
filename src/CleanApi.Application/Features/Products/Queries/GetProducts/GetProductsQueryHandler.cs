using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Common.Models;
using CleanApi.Domain.Common;

namespace CleanApi.Application.Features.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductResponse>>
{
    public async Task<Result<PagedResult<ProductResponse>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await productRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, cancellationToken);

        return new PagedResult<ProductResponse>(items.ToResponses(), request.Page, request.PageSize, totalCount);
    }
}
