using Microsoft.Extensions.DependencyInjection;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Application.Common.Exceptions;
using CleanApi.Application.Features.Products.Commands.CreateProduct;

namespace CleanApi.Application.UnitTests.Messaging;

/// <summary>Verifies the DI wiring: handler discovery and the validation pipeline.</summary>
public sealed class SenderTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddScoped(_ => Substitute.For<IProductRepository>());
        services.AddScoped(_ => Substitute.For<IUnitOfWork>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public async Task Send_DispatchesToHandler()
    {
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new CreateProductCommand("Keyboard", null, 10m), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Send_WithInvalidRequest_ThrowsValidationException()
    {
        await using var provider = BuildProvider();
        await using var scope = provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            sender.Send(new CreateProductCommand("", null, -1m), TestContext.Current.CancellationToken));

        Assert.Contains("Name", exception.Errors.Keys);
        Assert.Contains("Price", exception.Errors.Keys);
    }
}
