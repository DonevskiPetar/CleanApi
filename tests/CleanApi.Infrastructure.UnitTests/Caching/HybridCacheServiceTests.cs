using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using CleanApi.Domain.Common;
using CleanApi.Infrastructure.Caching;

namespace CleanApi.Infrastructure.UnitTests.Caching;

/// <summary>Runs against a real in-memory HybridCache (no Redis needed); Redis only adds a shared L2 behind it.</summary>
public sealed class HybridCacheServiceTests : IAsyncDisposable
{
    private static readonly Error NotFound = Error.NotFound("Test.NotFound", "Not found.");

    private readonly ServiceProvider _provider;
    private readonly HybridCacheService _sut;
    private int _factoryCalls;

    public HybridCacheServiceTests()
    {
        _provider = new ServiceCollection().AddHybridCache().Services.BuildServiceProvider();
        _sut = new HybridCacheService(
            _provider.GetRequiredService<HybridCache>(),
            Options.Create(new CachingOptions()),
            NullLogger<HybridCacheService>.Instance);
    }

    [Fact]
    public async Task GetOrCreateAsync_SuccessfulResult_IsCached()
    {
        var first = await _sut.GetOrCreateAsync("key", Factory(Result.Success("value")), ["tag"], cancellationToken: Ct);
        var second = await _sut.GetOrCreateAsync("key", Factory(Result.Success("other")), ["tag"], cancellationToken: Ct);

        Assert.Equal("value", first.Value);
        Assert.Equal("value", second.Value);
        Assert.Equal(1, _factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateAsync_FailedResult_IsReturnedButNotCached()
    {
        var failed = await _sut.GetOrCreateAsync("key", Factory(Result.Failure<string>(NotFound)), ["tag"], cancellationToken: Ct);
        var retried = await _sut.GetOrCreateAsync("key", Factory(Result.Success("value")), ["tag"], cancellationToken: Ct);

        Assert.True(failed.IsFailure);
        Assert.Equal(NotFound, failed.Error);
        Assert.Equal("value", retried.Value);
        Assert.Equal(2, _factoryCalls);
    }

    [Fact]
    public async Task RemoveByTagsAsync_EvictsTaggedEntries_AndKeepsOthers()
    {
        await _sut.GetOrCreateAsync("a", Factory(Result.Success("a1")), ["evict"], cancellationToken: Ct);
        await _sut.GetOrCreateAsync("b", Factory(Result.Success("b1")), ["keep"], cancellationToken: Ct);

        await _sut.RemoveByTagsAsync(["evict"], Ct);

        var a = await _sut.GetOrCreateAsync("a", Factory(Result.Success("a2")), ["evict"], cancellationToken: Ct);
        var b = await _sut.GetOrCreateAsync("b", Factory(Result.Success("b2")), ["keep"], cancellationToken: Ct);

        Assert.Equal("a2", a.Value);
        Assert.Equal("b1", b.Value);
    }

    [Fact]
    public async Task GetOrCreateAsync_ComplexValue_RoundTrips()
    {
        var value = new Sample(Guid.NewGuid(), "name", 9.99m, [1, 2, 3]);

        var result = await _sut.GetOrCreateAsync("complex", Factory(Result.Success(value)), [], cancellationToken: Ct);

        Assert.Equal(value.Id, result.Value.Id);
        Assert.Equal(value.Items, result.Value.Items);
    }

    public ValueTask DisposeAsync() => _provider.DisposeAsync();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Func<CancellationToken, Task<Result<T>>> Factory<T>(Result<T> result) => _ =>
    {
        Interlocked.Increment(ref _factoryCalls);
        return Task.FromResult(result);
    };

    public sealed record Sample(Guid Id, string Name, decimal Price, IReadOnlyList<int> Items);
}
