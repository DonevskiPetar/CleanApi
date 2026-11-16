using CleanApi.Domain.Products;

namespace CleanApi.Domain.UnitTests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Create_SetsPropertiesAndGeneratesId()
    {
        var product = Product.Create("Keyboard", "Mechanical", 89.99m);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal("Mechanical", product.Description);
        Assert.Equal(89.99m, product.Price);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var first = Product.Create("A", null, 1m);
        var second = Product.Create("B", null, 1m);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Update_ChangesValuesButKeepsId()
    {
        var product = Product.Create("Keyboard", "Mechanical", 89.99m);
        var originalId = product.Id;

        product.Update("Keyboard Pro", null, 129m);

        Assert.Equal(originalId, product.Id);
        Assert.Equal("Keyboard Pro", product.Name);
        Assert.Null(product.Description);
        Assert.Equal(129m, product.Price);
    }
}
