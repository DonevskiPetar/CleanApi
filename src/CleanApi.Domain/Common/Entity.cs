namespace CleanApi.Domain.Common;

/// <summary>
/// Base class for all entities. Audit fields are set automatically by the persistence layer.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.CreateVersion7();

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
