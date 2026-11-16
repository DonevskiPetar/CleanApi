using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        return product is null
            ? ProductErrors.NotFound(request.Id)
            : product.ToResponse();
    }
}
