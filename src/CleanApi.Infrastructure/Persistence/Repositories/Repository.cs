using Microsoft.EntityFrameworkCore;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Domain.Common;

namespace CleanApi.Infrastructure.Persistence.Repositories;

internal class Repository<TEntity>(AppDbContext dbContext) : IRepository<TEntity>
    where TEntity : Entity
{
    protected AppDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(e => e.Id == id, cancellationToken);

    public void Add(TEntity entity) => Set.Add(entity);

    public void Update(TEntity entity) => Set.Update(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);
}
