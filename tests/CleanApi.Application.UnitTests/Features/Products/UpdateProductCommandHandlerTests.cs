using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Features.Products.Commands.UpdateProduct;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenProductExists_UpdatesAndSaves()
    {
        var product = Product.Create("Keyboard", null, 10m);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _handler.Handle(
            new UpdateProductCommand(product.Id, "Keyboard Pro", "RGB", 20m), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Keyboard Pro", result.Value.Name);
        Assert.Equal(20m, product.Price);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var result = await _handler.Handle(
            new UpdateProductCommand(id, "Keyboard", null, 10m), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound(id), result.Error);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNewNameBelongsToAnotherProduct_ReturnsConflict()
    {
        var product = Product.Create("Keyboard", null, 10m);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.NameExistsAsync("Mouse", product.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(
            new UpdateProductCommand(product.Id, "Mouse", null, 10m), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("Keyboard", product.Name); // unchanged
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
