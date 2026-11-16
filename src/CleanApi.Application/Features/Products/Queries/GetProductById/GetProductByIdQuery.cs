using CleanApi.Application.Abstractions.Messaging;

namespace CleanApi.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
