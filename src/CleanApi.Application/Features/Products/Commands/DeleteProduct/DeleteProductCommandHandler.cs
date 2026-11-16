using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.Features.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(request.Id);
        }

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
