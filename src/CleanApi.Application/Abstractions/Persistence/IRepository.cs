using CleanApi.Domain.Common;

namespace CleanApi.Application.Abstractions.Persistence;

/// <summary>
/// Generic repository for aggregate roots. Changes are persisted by <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// Put entity-specific queries in a derived interface (see <c>IProductRepository</c>).
/// </summary>
public interface IRepository<TEntity>
    where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
