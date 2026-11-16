using CleanApi.Application.Features.Products.Commands.CreateProduct;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var result = _validator.Validate(new CreateProductCommand("Keyboard", null, 10m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("Keyboard", -1)]
    public void Validate_WithInvalidCommand_Fails(string name, decimal price)
    {
        var result = _validator.Validate(new CreateProductCommand(name, null, price));

        Assert.False(result.IsValid);
    }
}
