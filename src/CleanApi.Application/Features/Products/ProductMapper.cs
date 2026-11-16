using Riok.Mapperly.Abstractions;
using CleanApi.Domain.Products;

namespace CleanApi.Application.Features.Products;

/// <summary>
/// Compile-time generated mapper (Mapperly): no reflection, and an unmapped property is a build error.
/// </summary>
[Mapper]
public static partial class ProductMapper
{
    public static partial ProductResponse ToResponse(this Product product);

    public static partial IReadOnlyList<ProductResponse> ToResponses(this IEnumerable<Product> products);
}
