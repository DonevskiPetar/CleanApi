using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Features.Products.Commands.DeleteProduct;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductExists_RemovesAndSaves()
    {
        var product = Product.Create("Keyboard", null, 10m);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _handler.Handle(new DeleteProductCommand(product.Id), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        _productRepository.Received(1).Remove(product);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var result = await _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _productRepository.DidNotReceive().Remove(Arg.Any<Product>());
    }
}
