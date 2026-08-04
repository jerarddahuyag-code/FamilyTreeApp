# FamilyTreeApp — Technical Design

> **Status:** Living document. Updated at the end of each phase.
> **Last updated:** Phase 3 Roster complete

---

## 1. Unit Testing Strategy

Unit tests focus on domain invariants and command-handler business behavior. They cover:
- Domain entity creation and validation rules (e.g., invalid email or tree names)
- Domain state transitions (e.g., successful tree detail updates)
- Handler behavior for successful command execution and domain-guard failures

These tests avoid infrastructure-specific concerns such as cancellation-token propagation.

---

## 2. Solution Structure

```
FamilyTreeApp/                              ← Solution root
├── .editorconfig                           ✅ EXISTS
├── .globalconfig                           ✅ EXISTS
├── .github/workflows/ci.yml                ✅ EXISTS
├── docker-compose.yml                      ✅ EXISTS — Redis service added
├── requirements.md                         ✅ EXISTS
├── design.md                               ✅ EXISTS
├── tasks.md                                ✅ EXISTS
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
	│   │       ├── Error.cs                ✅ (Contains ErrorType enum)
	│   │       └── DomainErrors.cs         ✅
	│   ├── Users/
	│   │   ├── Entities/User.cs            ✅
	│   │   ├── Entities/ExternalLogin.cs   ✅
	│   │   └── Enums/Gender.cs             ✅
	│   ├── Trees/
	│   │   ├── Entities/Tree.cs            ✅
	│   │   └── Entities/TreeRbac.cs        ✅
	│   ├── Roster/
	│   │   ├── Entities/FamilyMember.cs    ✅
	│   │   ├── Entities/FamilyMemberRelationship.cs ✅
	│   │   └── Enums/
	│   │       ├── VisibilityStatus.cs     ✅
	│   │       └── RelationshipType.cs     ✅
	│   └── ValueObjects/ProfileInfo.cs     ✅ (Implemented as a C# record)
	│
	├── FamilyTreeApp.Application/          ✅ EXISTS
	│   ├── Common/
	│   │   ├── Behaviors/
	│   │   │   ├── LoggingBehavior.cs              ✅
	│   │   │   └── TransactionBehavior.cs          ✅
	│   │   └── Interfaces/
	│   │       ├── IApplicationDbContext.cs         ✅
	│   │       ├── ITransactionalCommand.cs         ✅
	│   │       ├── INotificationPublisher.cs        ✅
	│   │       └── INotificationHandler.cs          ✅
	│   ├── Users/CQRS/                             ✅ (Includes Users CRUD & Profile Update)
	│   ├── Trees/CQRS/                             ✅
	│   ├── Roster/CQRS/                            ✅
	│   └── DependencyInjection.cs                  ✅ (Scrutor decoration)
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
	│   ├── Authorization/                  ✅
	│   │   ├── TreeRoles.cs                ✅
	│   │   ├── TreeOwnerRequirement.cs     ✅
	│   │   ├── TreeAdminRequirement.cs     ✅
	│   │   ├── TreeMemberRequirement.cs    ✅
	│   │   └── TreeAuthorizationHandler.cs ✅
	│   ├── Controllers/
	│   │   ├── ApiControllerBase.cs        ✅
	│   │   ├── AuthController.cs           ✅
	│   │   ├── TreesController.cs          ✅
	│   │   ├── UsersController.cs          ✅
	│   │   ├── ProfileController.cs        ✅
	│   │   └── RosterController.cs         ✅
	│   ├── Middleware/GlobalExceptionHandler.cs  ✅
	│   ├── Dockerfile                      ✅
	│   └── Program.cs                      ✅ (Configures Swagger/OpenAPI)
	│
	└── FamilyTreeApp.Tests/                ✅ EXISTS
```

---

## 3. Project Dependency Graph

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

## 4. Behavior Pipeline

### 4.1 Input Validation Strategy

**Input validation is handled through two complementary approaches:**

1. **Domain Factory Validation**: Entity creation and modification via static factory methods (e.g., `User.Create(...)`, `Tree.Create(...)`) with private constructors. These factories return `Result<T>` with domain-specific errors.

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

### 4.2 Structural Pattern

All behaviors follow the **identical structural contract**.

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

### 4.3 DI Registration Order and Execution Chain

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

### 4.4 ITransactionalCommand Placement

Placed in `FamilyTreeApp.Application.Common.Interfaces` — transaction management
is an application-layer concern, not a domain concern. Commands opt-in explicitly.

```csharp
public record CreateTreeCommand : IRequest<Guid>, ITransactionalCommand { ... }
```

---

## 5. Roster Architecture

The Roster module manages the members of a family tree.

- **FamilyMember Aggregate**: Manages `VisibilityStatus` via a state machine (`Hidden` -> `Pending` -> `Visible`). Transitions are controlled by tree admins.
- **Anonymous Masking**: If a member's visibility is not `Visible` and the requester is not an admin, the profile data is masked as `ProfileInfo.CreateAnonymous()`.
- **Read-Through Merge**: If a `FamilyMember` has a `ClaimedByUserId`, the associated `User.ProfileInfo` is merged over the member's profile data during queries, providing the most up-to-date information.

---

## 6. Error Handling

### 6.1 Layer Responsibilities

| Layer | Mechanism | Rule |
|---|---|---|
| **Domain** | `Result<T>` / `Result` | Never throws for business rule violations |
| **Application** | Returns `Result<T>` | Catches infra exceptions only if a meaningful domain error can be produced |
| **Infrastructure** | May throw | EF Core, network, external API failures are exceptional |
| **API** | `ApiControllerBase` | Maps `ErrorType` to corresponding HTTP status codes |

### 6.2 Error Type Taxonomy

```csharp
public enum ErrorType { Failure, Validation, NotFound, Unauthorized, Conflict }

// Domain/Common/Errors/Error.cs
public record Error(string Code, string Message, ErrorType Type)
{
	public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}

// Domain/Common/Errors/DomainErrors.cs
public static class DomainErrors { ... }
```

---

## 7. Authentication Design

**Current Implementation**: Google OAuth with ASP.NET Core Cookie Authentication.
- `AddGoogleOpenIdConnect` handles the OAuth flow.
- Session persisted as an encrypted cookie.
- `AuthController` orchestrates user upsert via `ProcessExternalLoginCommandHandler`.
- The `IAuthService` provides a thin wrapper for cookie sign-in via `SignInAsync`.

---

## 8. Data Access

**Direct DbContext Pattern**: All data access is performed directly via `IApplicationDbContext` rather than using the Repository pattern. This provides full access to LINQ, reduces boilerplate, simplifies DTO projections, and is used consistently across Trees, Users, and Roster modules.

---

## 9. API & Documentation

- **Swagger/OpenAPI**: Configured in `Program.cs` with an OpenID Connect security definition. Swagger UI is available at `/swagger` during development.
- **ApiControllerBase**: All controllers inherit from this base class, which provides helper methods for mapping `Result` to the appropriate `IActionResult` based on the `ErrorType`.

---

## 10. Decision Records

### Decision — 2026-08 — Direct DbContext over Repository Pattern
**Decision:** Remove all usages of the Repository pattern in favor of direct `IApplicationDbContext` access.
**Context:** The codebase contained a mix of patterns. Roster used repositories; Trees/Users used DbContext directly.
**Rationale:** Direct DbContext usage reduces boilerplate interfaces/classes, simplifies complex query formulation (like those needed for Canvas Phase 4), allows direct DTO projection, and aligns with the existing approach in the Trees/Users modules.
**Impact:** All repository interfaces and implementations have been removed.

### Decision — 2026-08 — ErrorType HTTP Mapping
**Decision:** `Error` records include an `ErrorType` enum that `ApiControllerBase` maps to HTTP status codes.
**Context:** Previously, errors were mapped generic 400 Bad Request responses.
**Rationale:** Type-safe mapping allows for standard HTTP semantics (e.g., 404 for NotFound, 409 for Conflict).
**Impact:** Handlers set `ErrorType` appropriately, and the API layer automatically returns the correct status code.

### Decision — 2026-08 — ProfileInfo as C# Record
**Decision:** `ProfileInfo` is implemented as a C# `record` with `init` properties.
**Context:** Plan originally implied inheriting from a `ValueObject` base class.
**Rationale:** C# records provide built-in structural equality, rendering a `ValueObject` base class redundant.
**Impact:** Simplified implementation of value objects.
