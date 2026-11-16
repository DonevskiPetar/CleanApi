using CleanApi.Application.Features.Products.Commands.CreateProduct;
using CleanApi.Application.Features.Products.Commands.UpdateProduct;

namespace CleanApi.Api.Contracts.Products;

// HTTP request bodies. Kept separate from commands so the public API contract can evolve independently.

public sealed record CreateProductRequest(string Name, string? Description, decimal Price)
{
    public CreateProductCommand ToCommand() => new(Name, Description, Price);
}

public sealed record UpdateProductRequest(string Name, string? Description, decimal Price)
{
    public UpdateProductCommand ToCommand(Guid id) => new(id, Name, Description, Price);
}
