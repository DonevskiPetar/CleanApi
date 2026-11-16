using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using CleanApi.Api.ExceptionHandling;
using CleanApi.Application.Common.Exceptions;

namespace CleanApi.Api.UnitTests.ExceptionHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task ValidationException_Returns400WithFieldErrors()
    {
        var exception = new ValidationException(new Dictionary<string, string[]>
        {
            ["Name"] = ["'Name' must not be empty."],
        });

        var (statusCode, body) = await HandleAsync(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("'Name' must not be empty.", body.GetProperty("errors").GetProperty("Name")[0].GetString());
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutLeakingDetails()
    {
        var (statusCode, body) = await HandleAsync(new InvalidOperationException("secret connection string"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal("An unexpected error occurred.", body.GetProperty("title").GetString());
        Assert.DoesNotContain("secret", body.GetRawText(), StringComparison.Ordinal);
    }

    private static async Task<(int StatusCode, JsonElement Body)> HandleAsync(Exception exception)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        await using var provider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = provider };
        httpContext.Response.Body = new MemoryStream();

        var handler = new GlobalExceptionHandler(
            provider.GetRequiredService<IProblemDetailsService>(),
            NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);
        Assert.True(handled);

        httpContext.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(
            httpContext.Response.Body, cancellationToken: TestContext.Current.CancellationToken);

        return (httpContext.Response.StatusCode, document.RootElement.Clone());
    }
}
