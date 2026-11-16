using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (await productRepository.NameExistsAsync(request.Name, cancellationToken: cancellationToken))
        {
            return ProductErrors.NameNotUnique(request.Name);
        }

        var product = Product.Create(request.Name, request.Description, request.Price);

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToResponse();
    }
}
