using CleanApi.Application.Features.Products;
using CleanApi.Domain.Products;

namespace CleanApi.Application.UnitTests.Features.Products;

public sealed class ProductMapperTests
{
    [Fact]
    public void ToResponse_MapsAllProperties()
    {
        var product = Product.Create("Keyboard", "Mechanical", 89.99m);
        product.CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        product.UpdatedAtUtc = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var response = product.ToResponse();

        Assert.Equal(
            new ProductResponse(product.Id, "Keyboard", "Mechanical", 89.99m, product.CreatedAtUtc, product.UpdatedAtUtc),
            response);
    }

    [Fact]
    public void ToResponses_MapsEveryItemInOrder()
    {
        Product[] products = [Product.Create("A", null, 1m), Product.Create("B", null, 2m)];

        var responses = products.ToResponses();

        Assert.Equal(["A", "B"], responses.Select(r => r.Name));
    }
}
