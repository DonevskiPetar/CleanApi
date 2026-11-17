# CleanApi: Clean Architecture Web API template

ASP.NET Core (.NET 10 LTS) Web API starter with a layered architecture, CQRS-style handlers,
compile-time mapping, validation, EF Core repositories and unit tests.

## Quick start

```bash
dotnet tool restore                 # installs dotnet-ef (local tool)
dotnet run --project src/CleanApi.Api # applies migrations in Development, opens /swagger
dotnet test --solution CleanApi.slnx
```

- Swagger UI: `http://localhost:5128/swagger`
- OpenAPI document: `/openapi/v1.json`
- Health check: `/health`
- Sample requests: `src/CleanApi.Api/CleanApi.Api.http`

## Use it as a template for new projects

```bash
# once, from this folder
dotnet new install .

# for every new project: "CleanApi" is replaced with your name in namespaces, projects and files
dotnet new cleanapi -n MyShop
cd MyShop && dotnet tool restore && dotnet run --project src/MyShop.Api
```

After changing the template source, run `dotnet new install . --force` to refresh it.
To remove it: `dotnet new uninstall <full path to this folder>`.

## Solution layout

```
src/
  CleanApi.Domain          Entities, Result/Error types, domain errors. Depends on nothing.
  CleanApi.Application     Use cases: commands/queries + handlers, validators, mappers,
                         and the interfaces (repositories, unit of work) Infrastructure implements.
  CleanApi.Infrastructure  EF Core DbContext, entity configurations, migrations, repositories.
  CleanApi.Api             Controllers, request contracts, exception handling, Program.cs (composition root).
tests/
  CleanApi.Domain.UnitTests        Entities and Result type.
  CleanApi.Application.UnitTests   Handlers (NSubstitute mocks), validators, mapper, mediator pipeline.
  CleanApi.Api.UnitTests           Global exception handler, Error → HTTP status mapping.
  CleanApi.Infrastructure.UnitTests HybridCache-backed cache service (in-memory, no Redis needed).
```

## Environments

| Environment   | Settings file                   | Swagger | Migrate on startup | Connection string from        |
|---------------|---------------------------------|---------|--------------------|-------------------------------|
| Development   | `appsettings.Development.json`  | yes     | yes                | the file (LocalDB)            |
| Test          | `appsettings.Test.json`         | yes     | yes                | `ConnectionStrings__Database` |
| Acceptance    | `appsettings.Acceptance.json`   | yes     | no (pipeline)      | `ConnectionStrings__Database` |
| Production    | `appsettings.Production.json`   | no      | no (pipeline)      | `ConnectionStrings__Database` |

- Select the environment with `ASPNETCORE_ENVIRONMENT` (defaults to `Production` when not set).
- `appsettings.json` holds the shared defaults; the environment file overrides it; environment variables override both.
- Real connection strings and secrets never go in these files. Set them on the host (environment variable,
  Azure App Settings / Key Vault, ...). The app fails at startup with a clear message if the connection string is missing.
- The switches are `OpenApi:Enabled` and `Database:ApplyMigrationsOnStartup`.
- To deploy migrations from a pipeline, generate an idempotent script:
  `dotnet ef migrations script --idempotent --project src/CleanApi.Infrastructure --startup-project src/CleanApi.Api -o migrate.sql`

Dependency direction: `Api → Application → Domain` and `Infrastructure → Application`. `Api` references
`Infrastructure` only to wire it up in `Program.cs`.

### Request flow

```
HTTP → ProductsController → ISender.Send(command)
     → LoggingBehavior → ValidationBehavior → CreateProductCommandHandler
     → IProductRepository / IUnitOfWork (EF Core)
     ← Result<ProductResponse> (mapped by ProductMapper) → 200/201 or ProblemDetails
```

- **Handlers** return `Result` / `Result<T>` for expected failures (not found, conflict). The base
  `ApiController.Problem(Error)` maps `ErrorType` to 400/401/403/404/409 problem details with an `errorCode`.
- **Validation** failures throw `ValidationException`, which `GlobalExceptionHandler` turns into a 400
  `ValidationProblemDetails`. Unexpected exceptions are logged and become a generic 500.
- **Mediator** is a small built-in `ISender` (`Application/Abstractions/Messaging`), so there is no MediatR
  license dependency. Pipeline behaviors run in registration order.
- **Mapping** uses [Mapperly](https://mapperly.riok.app/) (source-generated, no reflection).
- **Dependency injection**: each layer has its own `DependencyInjection.cs` (`AddPresentation`,
  `AddApplication`, `AddInfrastructure`). Handlers and validators are discovered automatically.
- **Auditing**: `CreatedAtUtc` / `UpdatedAtUtc` are set by `AuditableEntityInterceptor`.
- **Package versions** live in `Directory.Packages.props`; shared build settings live in `Directory.Build.props`
  (warnings are treated as errors).

## Adding a feature (e.g. Orders)

1. **Domain**: `Domain/Orders/Order.cs` (inherit `Entity`) and `OrderErrors.cs`.
2. **Application**:
   - `Abstractions/Persistence/IOrderRepository.cs : IRepository<Order>` (only if you need custom queries;
     otherwise inject `IRepository<Order>`).
   - `Features/Orders/OrderResponse.cs`, `OrderMapper.cs` (`[Mapper]`).
   - `Features/Orders/Commands/CreateOrder/` → `CreateOrderCommand : ICommand<OrderResponse>`,
     `CreateOrderCommandValidator`, `CreateOrderCommandHandler : ICommandHandler<...>`.
   - Queries go under `Features/Orders/Queries/...` with `IQuery<T>` / `IQueryHandler<,>`.
   - Handlers and validators are registered automatically.
3. **Infrastructure**:
   - `Persistence/Configurations/OrderConfiguration.cs` (picked up automatically).
   - `Persistence/Repositories/OrderRepository.cs` + register it in `Infrastructure/DependencyInjection.cs`.
   - Add a migration:
     ```bash
     dotnet ef migrations add AddOrders --project src/CleanApi.Infrastructure --startup-project src/CleanApi.Api --output-dir Persistence/Migrations
     ```
4. **Api**: `Contracts/Orders/...Request.cs` and `Controllers/OrdersController.cs : ApiController`.

## Database

SQL Server via **LocalDB** (installed with Visual Studio). The database is created and migrated
automatically on the first run in Development. LocalDB starts on demand; nothing needs to be running beforehand.

- Connect with SSMS: server `(localdb)\MSSQLLocalDB`, Windows Authentication, database `CleanApi`.
- Connection string: `ConnectionStrings:Database` in `src/CleanApi.Api/appsettings.json`.
- Using a full SQL Server instead (Developer/Express/Docker/Azure SQL): change only the connection string,
  e.g. `Server=localhost;Database=CleanApi;Trusted_Connection=True;TrustServerCertificate=True`.
- For secrets (passwords) don't edit appsettings.json; use user secrets:
  `dotnet user-secrets set "ConnectionStrings:Database" "<connection string>" --project src/CleanApi.Api`.
- PostgreSQL: swap the provider package for `Npgsql.EntityFrameworkCore.PostgreSQL`, replace `UseSqlServer`,
  delete `Persistence/Migrations` and add a new initial migration.

## Caching (Redis)

Queries are cached with [HybridCache](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid): an in-memory
L1 in each instance and Redis as the shared L2. Redis is optional. When `ConnectionStrings:Redis` is empty, each
instance uses its own in-memory cache, which is fine for local development and single-instance hosting.

```bash
docker run -d --name redis -p 6379:6379 redis:7-alpine
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,abortConnect=false" --project src/CleanApi.Api
# other environments: ConnectionStrings__Redis="<host>:6380,password=<key>,ssl=True,abortConnect=False"
```

- **Cache a query**: implement `ICachedQuery<T>` instead of `IQuery<T>` and give it a `CacheKey` and `CacheTags`
  (optionally a `CacheExpiration`). `CachingBehavior` runs after validation, and only successful results are cached
  (a `NotFound` is never cached).
- **Invalidate**: implement `ICacheInvalidatingCommand` on the command and list the tags it makes stale.
  `CacheInvalidationBehavior` evicts them after the handler succeeds. For products, keys and tags live in
  `ProductCacheKeys`: `GetProductById` is tagged `products:{id}`, and every list page is tagged `products:list`.
- **Settings** (`Caching` section): `DefaultExpiration` (Redis TTL, 5 min), `LocalExpiration` (in-memory TTL, 1 min)
  and `InstanceName` (Redis key prefix).
- **Multiple instances**: tag invalidation reaches Redis and the instance that handled the write. Other instances keep
  their in-memory copy until `LocalExpiration`, so keep that value short.
- Values are serialized with System.Text.Json, so cached response types must round-trip through JSON.

## Ideas for next steps

Authentication (JWT), API versioning, CORS, rate limiting, integration tests with `Microsoft.AspNetCore.Mvc.Testing`,
Docker support, OpenTelemetry.
