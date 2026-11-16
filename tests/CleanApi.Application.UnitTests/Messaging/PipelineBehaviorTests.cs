using Microsoft.Extensions.DependencyInjection;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Domain.Common;

namespace CleanApi.Application.UnitTests.Messaging;

/// <summary>Shows how to test the mediator in isolation with your own request, handler and behaviors.</summary>
public sealed class PipelineBehaviorTests
{
    [Fact]
    public async Task Behaviors_RunInRegistrationOrder_AroundTheHandler()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(log);
        services.AddScoped<ISender, Sender>();
        services.AddScoped<IRequestHandler<PingQuery, Result<string>>, PingQueryHandler>();
        services.AddScoped<IPipelineBehavior<PingQuery, Result<string>>, OuterBehavior>();
        services.AddScoped<IPipelineBehavior<PingQuery, Result<string>>, InnerBehavior>();
        await using var provider = services.BuildServiceProvider();

        var result = await provider.GetRequiredService<ISender>()
            .Send(new PingQuery(), TestContext.Current.CancellationToken);

        Assert.Equal("pong", result.Value);
        Assert.Equal(["outer:before", "inner:before", "handler", "inner:after", "outer:after"], log);
    }

    public sealed record PingQuery : IQuery<string>;

    private sealed class PingQueryHandler(List<string> log) : IQueryHandler<PingQuery, string>
    {
        public Task<Result<string>> Handle(PingQuery request, CancellationToken cancellationToken)
        {
            log.Add("handler");
            return Task.FromResult<Result<string>>("pong");
        }
    }

    private sealed class OuterBehavior(List<string> log) : IPipelineBehavior<PingQuery, Result<string>>
    {
        public async Task<Result<string>> Handle(
            PingQuery request, RequestHandlerDelegate<Result<string>> next, CancellationToken cancellationToken)
        {
            log.Add("outer:before");
            var response = await next();
            log.Add("outer:after");
            return response;
        }
    }

    private sealed class InnerBehavior(List<string> log) : IPipelineBehavior<PingQuery, Result<string>>
    {
        public async Task<Result<string>> Handle(
            PingQuery request, RequestHandlerDelegate<Result<string>> next, CancellationToken cancellationToken)
        {
            log.Add("inner:before");
            var response = await next();
            log.Add("inner:after");
            return response;
        }
    }
}
