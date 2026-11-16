using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Features.Products.Commands.CreateProduct;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNameIsUnique_AddsProductAndSaves()
    {
        var command = new CreateProductCommand("Keyboard", "Mechanical", 89.99m);

        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Keyboard", result.Value.Name);
        _productRepository.Received(1).Add(Arg.Is<Product>(p => p.Name == "Keyboard" && p.Price == 89.99m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ReturnsConflictAndDoesNotSave()
    {
        _productRepository
            .NameExistsAsync("Keyboard", Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(
            new CreateProductCommand("Keyboard", null, 10m), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _productRepository.DidNotReceive().Add(Arg.Any<Product>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
