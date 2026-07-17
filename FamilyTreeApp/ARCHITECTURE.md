# Monolithic REST API Architecture

This project is built using a strict **Clean Architecture** combined with **Domain-Driven Design (DDD)** and **Command Query Responsibility Segregation (CQRS)** principles. 

The core ideology is dependency inversion: dependencies always point *inward* toward the Domain layer, ensuring that enterprise business rules remain independent of UI, databases, and external frameworks.

## Directory Breakdown

### 1. Domain Layer (`FamilyTreeApp.Domain`)
The most central layer. It contains all enterprise logic, entities, value objects, and domain interfaces. 
- **Zero Dependencies**: This layer has no project references and no external NuGet packages (not even MediatR).
- **Core Concepts**:
  - `Entity.cs`: The base class for domain models. Equality is based on a unique `Id` (Guid).
  - `AggregateRoot.cs`: A specialized Entity that serves as the entry point for a cluster of domain objects. Only Aggregate Roots can be retrieved directly from repositories. It also manages a collection of `IDomainEvent`s.
  - `ValueObject.cs`: Objects with no distinct identity. Equality is structural (based on their properties).
  - `IRepository<T>`: The standard contract for data access, restricted to `AggregateRoot`s.
  - CQRS Interfaces (`ICommandHandler`, `IQueryHandler`): Native C# interfaces used to dispatch use cases without relying on external mediator libraries.

### 2. Application Layer (`FamilyTreeApp.Application`)
Contains the application's use cases (CQRS commands and queries). It coordinates tasks and delegates work to the Domain layer.
- **Dependencies**: Depends only on the Domain layer.
- **CQRS Implementation**: We use native C# handlers. When you add a new feature, you implement `ICommandHandler<TCommand, TResult>` or `IQueryHandler<TQuery, TResult>`.
- **Pipeline Behaviors**: Cross-cutting concerns are handled via Decorators. For example, `ValidationPipelineBehavior` automatically runs FluentValidation on incoming commands before they reach your handler. These are automatically registered via `Scrutor`.

### 3. Infrastructure Layer (`FamilyTreeApp.Infrastructure`)
Contains the implementations for interfaces defined in the Domain and Application layers.
- **Dependencies**: Depends on the Application layer.
- **Persistence**: Implements the EF Core `ApplicationDbContext`, the generic `Repository<T>`, and the `UnitOfWork`.
- **Note**: The API layer injects this, but the Domain and Application layers are entirely unaware of EF Core.

### 4. API Layer (`FamilyTreeApp.Api`)
The presentation layer. Exposes REST endpoints to the outside world.
- **Dependencies**: Depends on Application and Infrastructure layers for dependency injection wire-up.
- **Controllers/Endpoints**: Should be extremely thin. Their only job is to receive HTTP requests, dispatch the appropriate Command/Query to the Application layer, and return the result. 
- **Exception Handling**: A `GlobalExceptionHandler` middleware intercepts domain exceptions (like `NotFoundException` or `ValidationException`) and translates them into appropriate HTTP status codes (404, 400).

---

## Sample Workflow: Implementing a New Feature

When implementing a new feature (e.g., "Create Order"), follow this exact vertical slice workflow:

1. **Domain Layer (The Core)**
   - Create a new `Order` entity inheriting from `AggregateRoot`.
   - Create any required `ValueObject`s (e.g., `Address`, `Money`).
   - Define a domain event like `OrderCreatedEvent`.
   - Create an `IOrderRepository` interface.

2. **Application Layer (The Use Case)**
   - Define a `CreateOrderCommand` record.
   - Implement `ICommandHandler<CreateOrderCommand, Guid>` inside a `CreateOrderCommandHandler` class.
   - (Optional) Create a `CreateOrderCommandValidator` inheriting from `AbstractValidator<CreateOrderCommand>`. Scrutor will automatically wire it up.

3. **Infrastructure Layer (The Implementation)**
   - Implement `OrderRepository : IOrderRepository` injecting the `ApplicationDbContext`.
   - Register the repository in `DependencyInjection.cs`.
   - Add the `DbSet<Order>` to the `ApplicationDbContext` and configure it in `OnModelCreating`.

4. **API Layer (The Entry Point)**
   - Create an `OrdersController`.
   - Inject `ICommandHandler<CreateOrderCommand, Guid>`.
   - Expose an `[HttpPost]` endpoint that accepts the command and passes it to the handler.

> [!TIP] 
> **Investigating Bugs**: Always start at the API Controller to check the payload, step into the Application Handler to verify orchestration and validation, and finally step into the Domain Entity to debug business logic failures.

---

## Extending Infrastructure

The infrastructure layer is designed to be plug-and-play. Here are comprehensive guides to extending it.

### 1. Swapping the Database (e.g., SQL Server to PostgreSQL)
1. In the `Infrastructure` project, remove the `Microsoft.EntityFrameworkCore.SqlServer` NuGet package.
2. Install the `Npgsql.EntityFrameworkCore.PostgreSQL` package.
3. Open `DependencyInjection.cs` in the Infrastructure project.
4. Change `options.UseSqlServer(...)` to `options.UseNpgsql(...)`.
5. Update your `appsettings.json` connection string format.
6. Delete the existing `Migrations` folder and run `dotnet ef migrations add InitialCreate` to generate PostgreSQL-specific migrations.

### 2. Adding Caching (Redis)
1. Install `Microsoft.Extensions.Caching.StackExchangeRedis` in the `Infrastructure` project.
2. In `Infrastructure/DependencyInjection.cs`, add:
   ```csharp
   services.AddStackExchangeRedisCache(options => {
       options.Configuration = configuration.GetConnectionString("Redis");
   });
   ```
3. In the `Application` layer, create an `ICacheService` interface.
4. In the `Infrastructure` layer, implement `RedisCacheService : ICacheService` injecting `IDistributedCache`.
5. Register `ICacheService` in DI.

### 3. Adding Authentication (JWT/OAuth)
1. Install `Microsoft.AspNetCore.Authentication.JwtBearer` in the `Api` project.
2. In `Api/Program.cs`, add:
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => {
           options.Authority = builder.Configuration["Jwt:Authority"];
           options.Audience = builder.Configuration["Jwt:Audience"];
       });
   builder.Services.AddAuthorization();
   ```
3. Add `app.UseAuthentication();` and `app.UseAuthorization();` to the pipeline before `app.MapControllers();`.
4. Apply `[Authorize]` attributes to your controllers.

### 4. Adding Logging and Observability (Serilog & OpenTelemetry)
**Serilog (Structured Logging)**
1. Install `Serilog.AspNetCore` in the `Api` project.
2. At the very top of `Program.cs`, configure Serilog:
   ```csharp
   Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
   builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console());
   ```
3. Add `app.UseSerilogRequestLogging();` in the middleware pipeline.

**OpenTelemetry (Tracing/Metrics)**
1. Install `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, and `OpenTelemetry.Instrumentation.Http` in the `Api` project.
2. In `Program.cs`, add:
   ```csharp
   builder.Services.AddOpenTelemetry()
       .WithTracing(tracing => tracing
           .AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddOtlpExporter())
       .WithMetrics(metrics => metrics
           .AddAspNetCoreInstrumentation()
           .AddOtlpExporter());
   ```
