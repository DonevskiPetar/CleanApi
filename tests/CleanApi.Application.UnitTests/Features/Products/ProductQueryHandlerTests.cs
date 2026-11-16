using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Features.Products.Queries.GetProductById;
using CleanApi.Application.Features.Products.Queries.GetProducts;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class ProductQueryHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();

    [Fact]
    public async Task GetProductById_WhenFound_ReturnsMappedProduct()
    {
        var product = Product.Create("Keyboard", "Mechanical", 89.99m);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var handler = new GetProductByIdQueryHandler(_productRepository);

        var result = await handler.Handle(new GetProductByIdQuery(product.Id), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(product.Id, result.Value.Id);
        Assert.Equal("Mechanical", result.Value.Description);
    }

    [Fact]
    public async Task GetProductById_WhenMissing_ReturnsNotFound()
    {
        var handler = new GetProductByIdQueryHandler(_productRepository);

        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetProducts_ReturnsPageWithPagingMetadata()
    {
        IReadOnlyList<Product> page = [Product.Create("A", null, 1m), Product.Create("B", null, 2m)];
        _productRepository.GetPagedAsync(2, 2, "x", Arg.Any<CancellationToken>()).Returns((page, 5));
        var handler = new GetProductsQueryHandler(_productRepository);

        var result = await handler.Handle(new GetProductsQuery(2, 2, "x"), TestContext.Current.CancellationToken);

        var paged = result.Value;
        Assert.Equal(["A", "B"], paged.Items.Select(p => p.Name));
        Assert.Equal(5, paged.TotalCount);
        Assert.Equal(3, paged.TotalPages);
        Assert.True(paged.HasPreviousPage);
        Assert.True(paged.HasNextPage);
    }
}
