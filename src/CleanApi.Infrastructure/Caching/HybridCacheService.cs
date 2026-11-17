using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CleanApi.Application.Abstractions.Caching;
using CleanApi.Domain.Common;

namespace CleanApi.Infrastructure.Caching;

/// <summary>
/// <see cref="ICacheService"/> on top of <see cref="HybridCache"/>: an in-memory L1 per instance and,
/// when <c>ConnectionStrings:Redis</c> is set, Redis as the shared L2. Values are serialized with System.Text.Json.
/// </summary>
internal sealed partial class HybridCacheService(
    HybridCache cache,
    IOptions<CachingOptions> options,
    ILogger<HybridCacheService> logger) : ICacheService
{
    private readonly CachingOptions _options = options.Value;

    public async Task<Result<T>> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        IReadOnlyCollection<string> tags,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = expiration ?? _options.DefaultExpiration,
            LocalCacheExpiration = Min(expiration ?? _options.DefaultExpiration, _options.LocalExpiration),
        };

        try
        {
            return await cache.GetOrCreateAsync(
                key,
                factory,
                static async (factory, ct) =>
                {
                    var result = await factory(ct);

                    // HybridCache stores whatever the factory returns. Throwing is the only way to skip storing,
                    // and the exception reaches every caller waiting on this key, each of which gets the failure back.
                    return result.IsSuccess ? result.Value : throw new UncacheableResultException(result);
                },
                entryOptions,
                tags,
                cancellationToken);
        }
        catch (UncacheableResultException ex)
        {
            return (Result<T>)ex.Result;
        }
    }

    public async Task RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveByTagAsync(tags, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The write that triggered this is already committed; failing the request now would only mislead the caller.
            // Stale entries still expire on their own (Caching:DefaultExpiration).
            LogInvalidationFailed(logger, ex);
        }
    }

    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;

    [LoggerMessage(Level = LogLevel.Error, Message = "Cache tag invalidation failed; entries stay stale until they expire")]
    private static partial void LogInvalidationFailed(ILogger logger, Exception exception);

    /// <summary>Carries a failed result out of the HybridCache factory so it is returned without being cached.</summary>
    private sealed class UncacheableResultException(Result result) : Exception("The result is a failure and is not cached.")
    {
        public Result Result { get; } = result;
    }
}
