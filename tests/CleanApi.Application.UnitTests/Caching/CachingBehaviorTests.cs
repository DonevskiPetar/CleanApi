using Microsoft.Extensions.DependencyInjection;
using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Common.Behaviors;
using CleanApi.Application.Features.Products;
using CleanApi.Application.Features.Products.Commands.CreateProduct;
using CleanApi.Application.Features.Products.Commands.DeleteProduct;
using CleanApi.Application.Features.Products.Commands.UpdateProduct;
using CleanApi.Application.Features.Products.Queries.GetProductById;
using CleanApi.Application.Features.Products.Queries.GetProducts;
using CleanApi.Domain.Common;
using CleanApi.Domain.Products;

namespace CleanApi.Application.UnitTests.Caching;

public sealed class CachingBehaviorTests
{
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    [Fact]
    public void AddApplication_CachedQuery_RunsCachingInnermost_AfterValidation()
    {
        using var provider = BuildProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<GetProductByIdQuery, Result<ProductResponse>>>()
            .Select(b => b.GetType().GetGenericTypeDefinition())
            .ToList();

        Assert.Equal(
            [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>), typeof(CacheInvalidationBehavior<,>), typeof(CachingBehavior<,>)],
            behaviors);
    }

    [Fact]
    public void AddApplication_Command_IsNotCached()
    {
        using var provider = BuildProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<CreateProductCommand, Result<ProductResponse>>>();

        Assert.DoesNotContain(behaviors, b => b.GetType().GetGenericTypeDefinition() == typeof(CachingBehavior<,>));
    }

    [Fact]
    public async Task CachingBehavior_UsesTheQueryKeyAndTags()
    {
        var id = Guid.NewGuid();
        var behavior = new CachingBehavior<GetProductByIdQuery, ProductResponse>(_cache);

        await behavior.Handle(new GetProductByIdQuery(id), () => throw new InvalidOperationException(), Ct);

        await _cache.Received(1).GetOrCreateAsync(
            ProductCacheKeys.ById(id),
            Arg.Any<Func<CancellationToken, Task<Result<ProductResponse>>>>(),
            Arg.Is<IReadOnlyCollection<string>>(t => t.SequenceEqual(new[] { ProductCacheKeys.ById(id) })),
            null,
            Ct);
    }

    [Fact]
    public void ProductListKeys_DifferPerPageSizeAndSearch_AndShareTheListTag()
    {
        ICachedQuery a = new GetProductsQuery(1, 20, "kb");
        ICachedQuery b = new GetProductsQuery(2, 20, "kb");
        ICachedQuery c = new GetProductsQuery(1, 20, null);

        Assert.Equal(3, new[] { a.CacheKey, b.CacheKey, c.CacheKey }.Distinct().Count());
        Assert.All([a, b, c], q => Assert.Equal([ProductCacheKeys.ListTag], q.CacheTags));
    }

    [Fact]
    public async Task CacheInvalidation_OnSuccess_EvictsTheCommandTags()
    {
        var id = Guid.NewGuid();
        var behavior = new CacheInvalidationBehavior<DeleteProductCommand, Result>(_cache);

        await behavior.Handle(new DeleteProductCommand(id), () => Task.FromResult(Result.Success()), Ct);

        await _cache.Received(1).RemoveByTagsAsync(
            Arg.Is<IEnumerable<string>>(t => t.SequenceEqual(new[] { ProductCacheKeys.ById(id), ProductCacheKeys.ListTag })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CacheInvalidation_OnFailure_LeavesTheCacheAlone()
    {
        var id = Guid.NewGuid();
        var behavior = new CacheInvalidationBehavior<UpdateProductCommand, Result<ProductResponse>>(_cache);

        await behavior.Handle(
            new UpdateProductCommand(id, "Name", null, 1m),
            () => Task.FromResult<Result<ProductResponse>>(ProductErrors.NotFound(id)),
            Ct);

        await _cache.DidNotReceiveWithAnyArgs().RemoveByTagsAsync(default!, Ct);
    }

    [Fact]
    public async Task CacheInvalidation_RequestWithoutTags_IsPassedThrough()
    {
        var behavior = new CacheInvalidationBehavior<GetProductByIdQuery, Result<ProductResponse>>(_cache);

        await behavior.Handle(
            new GetProductByIdQuery(Guid.NewGuid()),
            () => Task.FromResult<Result<ProductResponse>>(ProductErrors.NotFound(Guid.Empty)),
            Ct);

        await _cache.DidNotReceiveWithAnyArgs().RemoveByTagsAsync(default!, Ct);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ServiceProvider BuildProvider() => new ServiceCollection()
        .AddLogging()
        .AddApplication()
        .AddSingleton<ICacheService>(_cache)
        .AddSingleton<IProductRepository>(Substitute.For<IProductRepository>())
        .AddSingleton<IUnitOfWork>(Substitute.For<IUnitOfWork>())
        .BuildServiceProvider();
}
