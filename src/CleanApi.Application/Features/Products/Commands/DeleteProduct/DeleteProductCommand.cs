using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
