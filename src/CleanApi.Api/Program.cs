using CleanApi.Api;
using CleanApi.Application;
using CleanApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Each layer registers its own services (see DependencyInjection.cs in each project).
builder.Services
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Per-environment switches live in appsettings.{Environment}.json.
if (app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.MapOpenApi(); // OpenAPI document at /openapi/v1.json
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1")); // UI at /swagger
}

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

// Exposed so integration tests can reference the Program class (test server factory).
public partial class Program;
