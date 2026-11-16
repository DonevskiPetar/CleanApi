using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, string? Description, decimal Price) : ICommand<ProductResponse>;
