using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(request.Id);
        }

        if (await productRepository.NameExistsAsync(request.Name, request.Id, cancellationToken))
        {
            return ProductErrors.NameNotUnique(request.Name);
        }

        product.Update(request.Name, request.Description, request.Price);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToResponse();
    }
}
