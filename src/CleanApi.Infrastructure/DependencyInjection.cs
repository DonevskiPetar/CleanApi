using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CleanApi.Application.Abstractions.Caching;
using CleanApi.Application.Abstractions.Persistence;
using CleanApi.Infrastructure.Caching;
using CleanApi.Infrastructure.Persistence;
using CleanApi.Infrastructure.Persistence.Interceptors;
using CleanApi.Infrastructure.Persistence.Repositories;

namespace CleanApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddPersistence(configuration);
        services.AddCaching(configuration);

        return services;
    }

    /// <summary>
    /// HybridCache with an in-memory L1 and, when <c>ConnectionStrings:Redis</c> is set, Redis as the shared L2.
    /// Without a Redis connection string the app still runs, with an in-memory cache per instance.
    /// </summary>
    private static void AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CachingOptions.SectionName);
        services.Configure<CachingOptions>(section);
        var options = section.Get<CachingOptions>() ?? new CachingOptions();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            // HybridCache picks up the registered IDistributedCache as its L2.
            services.AddStackExchangeRedisCache(redis =>
            {
                redis.Configuration = redisConnectionString;
                redis.InstanceName = options.InstanceName;
            });
        }

        services.AddHybridCache(hybrid => hybrid.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = options.DefaultExpiration,
            LocalCacheExpiration = options.LocalExpiration,
        });

        services.AddSingleton<ICacheService, HybridCacheService>();
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' is not configured. Set it in appsettings.Development.json for local runs, " +
                "or with the ConnectionStrings__Database environment variable for other environments.");
        }

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
    }

    /// <summary>Applies pending EF Core migrations. Intended for local development only.</summary>
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
