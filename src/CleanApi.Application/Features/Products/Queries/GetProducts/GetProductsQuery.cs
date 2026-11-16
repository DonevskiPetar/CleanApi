using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Common.Models;

namespace CleanApi.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IQuery<PagedResult<ProductResponse>>;
