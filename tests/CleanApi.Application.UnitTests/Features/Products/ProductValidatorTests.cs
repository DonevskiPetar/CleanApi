using CleanApi.Application.Features.Products.Commands.UpdateProduct;
using CleanApi.Application.Features.Products.Queries.GetProducts;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class ProductValidatorTests
{
    [Fact]
    public void UpdateProduct_WithEmptyId_Fails()
    {
        var result = new UpdateProductCommandValidator().Validate(new UpdateProductCommand(Guid.Empty, "Keyboard", null, 10m));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProductCommand.Id));
    }

    [Fact]
    public void UpdateProduct_WithTooManyDecimals_Fails()
    {
        var result = new UpdateProductCommandValidator().Validate(new UpdateProductCommand(Guid.NewGuid(), "Keyboard", null, 10.123m));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProductCommand.Price));
    }

    [Fact]
    public void UpdateProduct_WithNameTooLong_Fails()
    {
        var result = new UpdateProductCommandValidator().Validate(
            new UpdateProductCommand(Guid.NewGuid(), new string('x', 201), null, 10m));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProductCommand.Name));
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(1, 100, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    public void GetProducts_ValidatesPaging(int page, int pageSize, bool expectedValid)
    {
        var result = new GetProductsQueryValidator().Validate(new GetProductsQuery(page, pageSize));

        Assert.Equal(expectedValid, result.IsValid);
    }
}
