namespace CleanApi.Application.Features.Products;

/// <summary>Cache keys and tags for products, shared by the queries that fill and the commands that evict them.</summary>
internal static class ProductCacheKeys
{
    /// <summary>Tag on every product list page (any page, size or search).</summary>
    public const string ListTag = "products:list";

    /// <summary>Key, and tag, of a single product.</summary>
    public static string ById(Guid id) => $"products:{id:N}";

    public static string List(int page, int pageSize, string? search) => $"{ListTag}:{page}:{pageSize}:{search}";
}
