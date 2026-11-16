using CleanApi.Api.ExceptionHandling;

namespace CleanApi.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        // ProductsController -> /api/products (also applies to generated links such as the Location header).
        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
            context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}");

        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();

        return services;
    }
}
