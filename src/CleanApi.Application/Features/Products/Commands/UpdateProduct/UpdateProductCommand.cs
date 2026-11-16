using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price)
    : ICommand<ProductResponse>;
