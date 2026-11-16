namespace CleanApi.Application.Abstractions.Messaging;

/// <summary>Dispatches a request to its handler through the registered pipeline behaviors.</summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
