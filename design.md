# FamilyTreeApp — Technical Design

> **Status:** Living document. Updated at the end of each phase.
> **Last updated:** Phase 0 / Phase 1 gap-filling pass

---

## 1. Solution Structure

### 1.1 Unit Testing Strategy

Unit tests focus on domain invariants and command-handler business behavior. They cover:
- Domain entity creation and validation rules (e.g., invalid email or tree names)
- Domain state transitions (e.g., successful tree detail updates)
- Handler behavior for successful command execution and domain-guard failures

These tests avoid infrastructure-specific concerns such as cancellation-token propagation.

---

## 2. Project Dependency Graph

```
FamilyTreeApp/                              ← Solution root
├── .editorconfig                           ✅ EXISTS
├── .globalconfig                           ✅ EXISTS (TASK-0.1)
├── .github/workflows/ci.yml               ✅ EXISTS (TASK-0.3)
├── docker-compose.yml                      ✅ EXISTS — Redis service added (TASK-0.2)
├── requirements.md                         ✅ EXISTS
├── design.md                               ✅ EXISTS
├── tasks.md                                ✅ EXISTS
├── implementation_plan.md                  ✅ EXISTS
└── FamilyTreeApp/
	├── FamilyTreeApp.Domain/               ✅ EXISTS — Zero external dependencies
	│   ├── Common/
	│   │   ├── IRequest.cs                 ✅
	│   │   ├── ICommandHandler.cs          ✅
	│   │   ├── IQueryHandler.cs            ✅
	│   │   ├── IUnitOfWork.cs              ✅
	│   │   ├── Result.cs                   ✅
	│   │   ├── Result{T}.cs                ✅
	│   │   ├── AggregateRoot.cs            ✅
	│   │   └── Errors/
	│   │       ├── Error.cs                ✅
	│   │       └── DomainErrors.cs         ✅
	│   ├── Users/
	│   │   ├── Entities/User.cs            ✅
	│   │   ├── Entities/ExternalLogin.cs   ✅
	│   │   └── Enums/Gender.cs             ✅
	│   ├── Trees/
	│   │   ├── Entities/Tree.cs            ✅
	│   │   └── Entities/TreeRbac.cs        ✅
	│   └── ValueObjects/ProfileInfo.cs     ✅
	│
	├── FamilyTreeApp.Application/          ✅ EXISTS
	│   ├── Common/
	│   │   ├── Behaviors/
	│   │   │   ├── LoggingBehavior.cs              ✅ (TASK-1.2)
	│   │   │   └── TransactionBehavior.cs          ✅ (TASK-1.3)
	│   │   └── Interfaces/
	│   │       ├── IApplicationDbContext.cs         ✅ EXISTS
	│   │       ├── ITransactionalCommand.cs         ✅ (TASK-1.1)
	│   │       ├── INotificationPublisher.cs        ✅ (TASK-1.4)
	│   │       └── INotificationHandler.cs          ✅ (TASK-1.5)
	│   ├── Users/CQRS/                             ✅
	│   ├── Trees/CQRS/                             ✅
	│   └── DependencyInjection.cs                  ✅ (TASK-1.6)
	│
	├── FamilyTreeApp.Infrastructure/       ✅ EXISTS
	│   ├── Persistence/
	│   │   ├── ApplicationDbContext.cs     ✅
	│   │   ├── UnitOfWork.cs               ✅
	│   │   ├── Model Configurations/       ✅
	│   │   └── Migrations/                 ✅
	│   ├── Services/AuthService.cs         ✅
	│   └── DependencyInjection.cs          ✅
	│
	├── FamilyTreeApp.Api/                  ✅ EXISTS
	│   ├── Controllers/
	│   │   ├── AuthController.cs           ✅
	│   │   └── TreesController.cs          ✅
	│   ├── Middleware/GlobalExceptionHandler.cs  ✅
	│   ├── Dockerfile                      ✅
	│   └── Program.cs                      ✅
	│
	└── FamilyTreeApp.Tests/                ✅ EXISTS (TASK-0.4)
		├── FamilyTreeApp.Tests.csproj
		└── Unit/
			├── Domain/
			│   ├── UserTests.cs            🔴 TODO (TASK-0.5)
			│   ├── TreeTests.cs            🔴 TODO (TASK-0.5)
			│   └── TreeRbacTests.cs        🔴 TODO (TASK-0.5)
			└── Application/
				├── Behaviors/
				│   ├── LoggingBehaviorTests.cs      ✅ (TASK-1.7)
				│   └── TransactionBehaviorTests.cs  ✅ (TASK-1.7)
				└── Trees/
					└── CreateTreeCommandHandlerTests.cs  🔴 TODO (TASK-0.5)
```

---

## 2. Project Dependency Graph

```
FamilyTreeApp.Domain
	↑
FamilyTreeApp.Application  ─────────────────────────────┐
	↑                                                    │
FamilyTreeApp.Infrastructure  →  FamilyTreeApp.Application
	↑
FamilyTreeApp.Api  →  FamilyTreeApp.Application
				   →  FamilyTreeApp.Infrastructure   (DI wire-up only, via AddInfrastructureServices)

FamilyTreeApp.Tests  →  FamilyTreeApp.Domain
					 →  FamilyTreeApp.Application
					 →  FamilyTreeApp.Infrastructure
```

> **Dependency Rule:** Domain has zero project or NuGet references.
> API references Infrastructure only for DI registration — never calls
> Infrastructure types directly.

---

## 3. Behavior Pipeline

### 3.1 Input Validation Strategy

**Input validation is handled through two complementary approaches:**

1. **Domain Factory Validation**: Entity creation and modification via static factory methods (e.g., `User.Create(...)`, `Tree.Create(...)`) that return `Result<T>` with domain-specific errors.

2. **Inline Guard Clauses**: Command handlers perform explicit validation checks for simple invariants (null, empty strings, etc.) before calling domain logic.

**Example patterns:**

```csharp
// Inline guard clause (handler-level)
if (string.IsNullOrWhiteSpace(command.Email))
{
	return Result.Failure<Guid>(DomainErrors.UserErrors.EmailRequired);
}

// Domain factory validation
Result<User> result = User.Create(command.Email, profileInfo);
if (result.IsFailure)
{
	return Result.Failure<Guid>(result.Error);
}
```

### 3.2 Structural Pattern

All behaviors follow the **identical structural contract**. This is non-negotiable — consistency enables contributors to understand the pipeline at a glance.

```csharp
// Namespace: FamilyTreeApp.Application.Common.Behaviors
public sealed class XyzBehavior<TRequest, TResponse>(
	ICommandHandler<TRequest, TResponse> innerHandler,
	IAdditionalDependency dep)                         // add as needed
	: ICommandHandler<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	public async Task<Result<TResponse>> HandleAsync(
		TRequest command,
		CancellationToken cancellationToken = default)
	{
		// pre-work
		Result<TResponse> result = await innerHandler.HandleAsync(command, cancellationToken);
		// post-work
		return result;
	}
}
```

### 3.3 DI Registration Order and Execution Chain

Scrutor `Decorate<>()` uses **LIFO** — last registered wraps outermost.

**Registration order in `DependencyInjection.cs`:**

```
Step 1 — Scrutor Scan registers raw handlers         (innermost layer)
Step 2 — Decorate with TransactionBehavior
Step 3 — Decorate with LoggingBehavior              (outermost layer)
```

**Runtime execution chain per command:**

```
HTTP Request
  └─▶ LoggingBehavior
		Starts Stopwatch.
		Calls inner handler.
		Stops Stopwatch.
		Logs Success @ Information or Failure @ Warning.
		Returns result.
		↓
  └─▶ TransactionBehavior
		If TRequest does NOT implement ITransactionalCommand → pass-through (zero overhead).
		If TRequest implements ITransactionalCommand:
		  BeginTransactionAsync → call handler → CommitAsync or RollbackAsync.
		Returns result.
		↓
  └─▶ Handler (concrete implementation)
		Input validation (inline guard clauses).
		Business logic (domain factory calls).
		Returns Result<TResponse>.
```

### 3.4 LoggingBehavior Design Detail

```csharp
public sealed class LoggingBehavior<TRequest, TResponse>(
	ICommandHandler<TRequest, TResponse> innerHandler,
	ILogger<LoggingBehavior<TRequest, TResponse>> logger)
	: ICommandHandler<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
```

| Concern | Decision |
|---|---|
| Timer | `System.Diagnostics.Stopwatch` — no external dependency |
| Log level on success | `Information` |
| Log level on failure | `Warning` |
| Fields logged | `{CommandType}`, `{ElapsedMs}`, `{ErrorCode}` (failure only), `{ErrorDescription}` (failure only) |
| PII policy | Request payload is **never** logged |
| Exception handling | Exceptions propagate unchanged — not caught by this behavior |

### 3.5 TransactionBehavior Design Detail

```csharp
public sealed class TransactionBehavior<TRequest, TResponse>(
	ICommandHandler<TRequest, TResponse> innerHandler,
	IApplicationDbContext dbContext)
	: ICommandHandler<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
```

| Concern | Decision |
|---|---|
| Transaction detection | `typeof(TRequest).IsAssignableTo(typeof(ITransactionalCommand))` |
| Non-transactional path | Direct pass-through, zero DB calls |
| Transaction scope | `dbContext.Database.BeginTransactionAsync()` |
| Commit condition | `result.IsSuccess == true` |
| Rollback condition | `result.IsFailure == true` OR exception thrown |
| Exception handling | Rollback then re-throw — behavior does not swallow exceptions |

### 3.6 ITransactionalCommand Placement

Placed in `FamilyTreeApp.Application.Common.Interfaces` — transaction management
is an application-layer concern, not a domain concern. Commands opt-in explicitly:

```csharp
public record CreateTreeCommand : IRequest<Guid>, ITransactionalCommand { ... }
```

---

## 4. Notification Infrastructure

Interfaces only — concrete implementation deferred to Phase 6.

```csharp
// FamilyTreeApp.Application.Common.Interfaces

public interface INotificationPublisher
{
	Task PublishAsync<T>(T notification, CancellationToken cancellationToken = default)
		where T : notnull;
}

public interface INotificationHandler<in T> where T : notnull
{
	Task HandleAsync(T notification, CancellationToken cancellationToken = default);
}
```

**Phase 6 implementation plan:** Register a concrete `NotificationPublisher` that
resolves all `INotificationHandler<T>` instances from DI and fan-outs via
`Task.WhenAll`.

---

## 5. Error Handling

### 5.1 Layer Responsibilities

| Layer | Mechanism | Rule |
|---|---|---|
| **Domain** | `Result<T>` / `Result` | Never throws for business rule violations |
| **Application** | Returns `Result<T>` | Catches infra exceptions only if a meaningful domain error can be produced |
| **Infrastructure** | May throw | EF Core, network, external API failures are exceptional |
| **API** | `GlobalExceptionHandler` | Maps `Result.IsFailure` → 4xx; unhandled exceptions → 500 RFC 7807 Problem Details |

### 5.2 Error Type Taxonomy

```csharp
// Domain/Common/Errors/Error.cs
public record Error(string Code, string Description)
{
	public static readonly Error None = new(string.Empty, string.Empty);
}

// Domain/Common/Errors/DomainErrors.cs
public static class DomainErrors
{
	public static class UserErrors { ... }
	public static class TreeErrors { ... }
	public static class Visibility { ... }    // Phase 3
	public static class Roster { ... }        // Phase 3
}
```

---

## 6. Docker Compose Infrastructure

`docker-compose.yml` hosts all local development dependencies.

| Service | Image | Host Port | Purpose |
|---|---|---|---|
| `database` | `pgvector/pgvector:pg18` | `5433` | PostgreSQL with pgvector extension |
| `backend` | Local Dockerfile | `8080` | ASP.NET Core API |
| `redis` | `redis:7-alpine` | `6379` | Cache (active Phase 6; provisioned Phase 0) |

Redis is provisioned in Phase 0 so that Phase 6 requires **zero** infrastructure
changes. It is unused until `IDistributedCache` is pointed at Redis.

---

## 7. CI/CD Pipeline

```
Trigger: push → main | pull_request → main

Jobs:
  build-and-test:
	runs-on: ubuntu-latest
	steps:
	  1. actions/checkout@v4
	  2. actions/setup-dotnet@v4          dotnet-version: '10.0.x'
	  3. dotnet restore
	  4. dotnet format --verify-no-changes --verbosity diagnostic
	  5. dotnet build --no-restore --warnaserror
	  6. dotnet test --no-build --verbosity normal
```

**Failure policy:** any step failure aborts the run. PRs cannot be merged
while the pipeline is red.

---

## 8. Test Design

### 8.1 Stack

| Concern | Library |
|---|---|
| Test framework | xUnit |
| Assertions | FluentAssertions |
| Mocking | NSubstitute |
| Discovery | `dotnet test` (xunit.runner.visualstudio) |

### 8.2 Unit Test Conventions

- **No database.** All unit tests use NSubstitute mocks or in-memory fakes.
- **Naming:** `MethodName_StateUnderTest_ExpectedBehaviour`
- **Arrangement:** AAA (Arrange / Act / Assert)
- **No shared state** between tests; each test constructs its own sut.

### 8.3 Test Coverage Targets (Phase 0)

| File | Scenarios |
|---|---|
| `UserTests.cs` | `Create` valid → success; empty email → failure; null profile → failure |
| `TreeTests.cs` | `Create` valid → success; empty name → failure; `UpdateDetails` empty name → failure |
| `TreeRbacTests.cs` | `Create` valid role → success; Owner passes Owner/Admin/Member checks |
| `CreateTreeCommandHandlerTests.cs` | Happy path → persists tree + returns Guid; duplicate name → failure |
| `LoggingBehaviorTests.cs` | Success → logger called at Information; failure → logger called at Warning |
| `TransactionBehaviorTests.cs` | Non-transactional → no transaction; success → commit; failure → rollback; throw → rollback + rethrow |

### 8.4 Integration Tests

Deferred to **Phase 8** (Testcontainers + `WebApplicationFactory`). Integration
tests are not part of the Phase 0 CI run.

---

## 9. Authentication Design

### 9.1 Current Implementation (Confirmed)

Google OAuth with ASP.NET Core Cookie Authentication:
- `AddGoogleOpenIdConnect` handles the OAuth flow
- Session persisted as an encrypted cookie (`ExpireTimeSpan = 14 days, SlidingExpiration = true`)
- `AuthController` handles the callback, calls `ProcessExternalLoginCommand` to upsert the User

### 9.2 User Upsert Flow

```
1. Browser → GET /auth/login → challenge Google
2. Google → callback → AuthController.Callback
3. AuthController extracts claims (sub, email, given_name, family_name, picture)
4. ProcessExternalLoginCommand dispatched:
   a. Look up ExternalLogin by (Provider="Google", SubjectId=sub)
   b. If found → return existing UserId
   c. If not found → create User + ExternalLogin, persist
5. Sign-in cookie issued by ASP.NET Core
6. Browser redirected to frontend
```

### 9.3 Phase 5 Hardening

When Phase 5 begins, all `[AllowAnonymous]` endpoints introduced during development
will be replaced with appropriate `[Authorize]` / RBAC policy attributes.

---

## 10. Decision Records

### Decision — 2025-07 — Keep FluentValidation
**Decision:** Retain `FluentValidation` via `ValidationPipelineBehavior` for input validation.
**Context:** Implementation plan originally specified manual guard clauses only. Team evaluated the existing codebase.
**Rationale:** FluentValidation is already integrated, auto-registered via Scrutor, and provides better test coverage for validators. Guard clauses remain in domain entities for invariant enforcement.
**Impact:** `ValidationPipelineBehavior` is the outermost behavior; FluentValidation validators are registered per-command.
**Review:** Revisit if validator count causes DI resolution overhead at scale.

---

### Decision — 2025-07 — Cookie Auth over Custom JWT
**Decision:** Use Google OAuth with ASP.NET Core cookie authentication; do not issue a custom JWT.
**Context:** Plan originally specified API-issued JWT + refresh token rotation.
**Rationale:** Cookie-based sessions are simpler, eliminate token storage concerns on the client, and rely on ASP.NET Core's battle-tested data protection stack.
**Impact:** Frontend must be a same-domain SPA or rely on the cookie credential. Cross-domain scenarios require re-evaluation.
**Review:** Reassess if a public API or mobile client is introduced.

---

### Decision — 2025-07 — Behaviors Not Decorators (Naming)
**Decision:** The pipeline pattern is called *behaviors*, not *decorators*.
**Rationale:** Consistent with `ValidationPipelineBehavior` already in the codebase; reduces cognitive overhead when adding future cross-cutting concerns.
**Impact:** All new pipeline classes live in `FamilyTreeApp.Application.Common.Behaviors`.
