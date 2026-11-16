using CleanApi.Domain.Common;

namespace CleanApi.Application.Abstractions.Messaging;

/// <summary>Marker for anything that can be sent through <see cref="ISender"/>.</summary>
public interface IRequest<TResponse>;

/// <summary>A request that changes state and returns no value.</summary>
public interface ICommand : IRequest<Result>;

/// <summary>A request that changes state and returns a value.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

/// <summary>A request that only reads state.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
