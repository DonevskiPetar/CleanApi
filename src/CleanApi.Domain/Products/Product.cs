using CleanApi.Domain.Common;

namespace CleanApi.Domain.Products;

/// <summary>
/// Sample aggregate. Replace or extend it with your own entities.
/// State changes go through methods so invariants stay in one place.
/// </summary>
public sealed class Product : Entity
{
    // Required by EF Core.
    private Product()
    {
    }

    private Product(string name, string? description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public static Product Create(string name, string? description, decimal price) =>
        new(name, description, price);

    public void Update(string name, string? description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }
}
