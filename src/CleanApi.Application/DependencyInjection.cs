using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Messaging;
using CleanApi.Application.Common.Behaviors;
using CleanApi.Domain.Common;

namespace CleanApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<ISender, Sender>();
        services.AddRequestHandlers(assembly);

        // Pipeline behaviors, outermost first.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
        services.AddCachingBehaviors(assembly);

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    /// <summary>
    /// Registers <see cref="CachingBehavior{TQuery,TValue}"/> for every <see cref="ICachedQuery{TResponse}"/> in the assembly.
    /// Registered per closed type because the behavior needs the query's value type to cache it.
    /// </summary>
    private static void AddCachingBehaviors(this IServiceCollection services, Assembly assembly)
    {
        var cachedQueries = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICachedQuery<>))
                .Select(i => (Query: t, Value: i.GetGenericArguments()[0])));

        foreach (var (query, value) in cachedQueries)
        {
            services.AddScoped(
                typeof(IPipelineBehavior<,>).MakeGenericType(query, typeof(Result<>).MakeGenericType(value)),
                typeof(CachingBehavior<,>).MakeGenericType(query, value));
        }
    }

    /// <summary>Registers every concrete <see cref="IRequestHandler{TRequest,TResponse}"/> in the assembly.</summary>
    private static void AddRequestHandlers(this IServiceCollection services, Assembly assembly)
    {
        var registrations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => (Service: i, Implementation: t)));

        foreach (var (service, implementation) in registrations)
        {
            services.AddScoped(service, implementation);
        }
    }
}
