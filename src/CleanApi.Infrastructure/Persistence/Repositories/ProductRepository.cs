using Microsoft.EntityFrameworkCore;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Domain.Products;

namespace CleanApi.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(AppDbContext dbContext) : Repository<Product>(dbContext), IProductRepository
{
    public Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(p => p.Name == name && (excludeId == null || p.Id != excludeId), cancellationToken);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
