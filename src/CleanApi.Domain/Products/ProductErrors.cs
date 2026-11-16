using CleanApi.Domain.Common;

namespace CleanApi.Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Products.NotFound", $"Product with id '{id}' was not found.");

    public static Error NameNotUnique(string name) =>
        Error.Conflict("Products.NameNotUnique", $"A product named '{name}' already exists.");
}
