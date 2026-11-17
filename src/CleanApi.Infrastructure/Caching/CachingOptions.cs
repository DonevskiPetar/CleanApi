namespace CleanApi.Infrastructure.Caching;

/// <summary>Settings bound from the <c>Caching</c> configuration section.</summary>
public sealed class CachingOptions
{
    public const string SectionName = "Caching";

    /// <summary>Default time to live in the distributed (Redis) cache.</summary>
    public TimeSpan DefaultExpiration { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Time to live in each instance's in-memory cache. Keep it short when running several instances:
    /// tag invalidation reaches Redis and the local instance, but other instances keep their in-memory copy until it expires.
    /// </summary>
    public TimeSpan LocalExpiration { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>Prefix for every Redis key, so several apps or environments can share one Redis.</summary>
    public string InstanceName { get; init; } = "CleanApi:";
}
