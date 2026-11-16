namespace CleanApi.Application.Abstractions.Persistence;

/// <summary>Commits all changes tracked by the repositories in the current scope as one transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
