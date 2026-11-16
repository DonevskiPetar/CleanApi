using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Domain.Common;

namespace CleanApi.Application.Common.Behaviors;

/// <summary>Logs every request, its duration and any failed <see cref="Result"/>.</summary>
internal sealed partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        LogHandling(logger, requestName);

        var startTimestamp = Stopwatch.GetTimestamp();
        var response = await next();
        var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

        if (response is Result { IsFailure: true } result)
        {
            LogFailed(logger, requestName, result.Error.Code, elapsedMs);
        }
        else
        {
            LogHandled(logger, requestName, elapsedMs);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs:0.0} ms")]
    private static partial void LogHandled(ILogger logger, string requestName, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{RequestName} failed with {ErrorCode} in {ElapsedMs:0.0} ms")]
    private static partial void LogFailed(ILogger logger, string requestName, string errorCode, double elapsedMs);
}
