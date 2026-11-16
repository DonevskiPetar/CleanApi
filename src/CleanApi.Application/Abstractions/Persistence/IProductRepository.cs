using CleanApi.Domain.Products;

namespace CleanApi.Application.Abstractions.Persistence;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default);
}
