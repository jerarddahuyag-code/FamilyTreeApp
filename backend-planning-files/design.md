# FamilyTreeApp — Technical Design

> **Status:** Living document. Updated at the end of each phase.
> **Last updated:** Phase 4 Canvas decisions recorded

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
	│   └── Canvas/
	│       ├── Entities/
	│       │   ├── TreeNode.cs             🔴 Phase 4
	│       │   ├── TreeNodeMember.cs       🔴 Phase 4
	│       │   └── TreeEdge.cs             🔴 Phase 4
	│       ├── ValueObjects/
	│       │   └── CanvasCoordinates.cs    🔴 Phase 4
	│       ├── Enums/
	│       │   └── NodeType.cs             🔴 Phase 4
	│       └── Services/
	│           └── VisibilityMediator.cs   🔴 Phase 4
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
	│   ├── Canvas/                                 🔴 Phase 4
	│   │   ├── Queries/GetCanvas/                  🔴 Phase 4
	│   │   ├── Commands/AddTreeNode/               🔴 Phase 4
	│   │   ├── Commands/AddTreeEdge/               🔴 Phase 4
	│   │   ├── Commands/RemoveTreeNode/            🔴 Phase 4
	│   │   ├── Commands/RemoveTreeEdge/            🔴 Phase 4
	│   │   ├── Commands/UpdateCanvas/              🔴 Phase 4
	│   │   └── DTOs/                              🔴 Phase 4
	│   └── DependencyInjection.cs                  ✅ (Scrutor decoration)
	│
	├── FamilyTreeApp.Infrastructure/       ✅ EXISTS
	│   ├── Persistence/
	│   │   ├── ApplicationDbContext.cs     ✅
	│   │   ├── UnitOfWork.cs               ✅
	│   │   ├── Model Configurations/       ✅ (+ Canvas configs Phase 4)
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
	│   │   ├── RosterController.cs         ✅
	│   │   └── CanvasController.cs         🔴 Phase 4
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

## 6. Canvas Architecture (Phase 4)

The Canvas module provides the visual mapping layer for a family tree. It is strictly decoupled from the Roster (Biological) layer.

### 6.1 Three-Layer Model

```
Biological Layer (Roster)          Visual Layer (Canvas)
─────────────────────────          ──────────────────────
FamilyMember            ◄── via TreeNodeMember join ──► TreeNode
FamilyMemberRelationship                               TreeEdge
```

- `TreeNode` represents a visual card on the canvas. `NodeType` = `Single | Partner | MultiPerson`.
- `TreeNodeMember` is a join table linking `TreeNode` to one or more `FamilyMember`s. A `FamilyMember` MAY appear on multiple `TreeNode`s (duplication is permitted).
- `TreeEdge` represents a visual connection between two `TreeNode`s.
- Canvas CRUD commands **MUST NOT** create or mutate Roster entities. Layer segregation is enforced at the Application command level.

### 6.2 CanvasDto Payload (Response Shape)

```json
{
  "value": {
    "nodes": [
      {
        "id": "uuid",
        "type": "Single",
        "position": { "x": 150.0, "y": 250.0 },
        "isMasked": false,
        "members": [
          {
            "id": "uuid",
            "profileInfo": { "firstName": "John", "lastName": "Doe" },
            "visibilityStatus": "Visible"
          }
        ]
      }
    ],
    "edges": [
      { "id": "uuid", "sourceNodeId": "uuid", "targetNodeId": "uuid" }
    ]
  },
  "isSuccess": true
}
```

> Note: If `isMasked` is `true`, the member's `profileInfo` will only contain `{ "firstName": "Anonymous" }`.

### 6.3 VisibilityMediator Domain Service

- Input: list of `TreeNode` entities (with loaded `TreeNodeMember → FamilyMember → User` chain) + requesting user's authorization context.
- Precomputes per-member masking: if `VisibilityStatus != Visible` AND the requester lacks `TreeAdmin` role → mark as masked.
- Returns a decorated list of nodes with visibility applied.

### 6.4 Data Fetching & Caching Strategy

| Concern | Approach |
|---------|----------|
| Backend query | `GetCanvasQueryHandler` runs fresh DB query per request |
| Server-side caching | None in Phase 4; deferred to Phase 6 (Redis) |
| Frontend caching | TanStack Query (stale-while-revalidate) |
| Cache invalidation | TanStack Query cache invalidated after any canvas mutation |

### 6.5 Canvas Editing & Save Strategy

| Trigger | Behavior |
|---------|----------|
| Node drag | React Flow updates local state only (no API call) |
| "Save Layout" button | Dispatches `PUT /api/v1/trees/{treeId}/canvas` with bulk `(NodeId, X, Y)` payload |
| 5-minute auto-save | Background timer fires if canvas has unsaved changes; shows "Saving..." toast |
| Mutation success | TanStack Query invalidates `canvas:{treeId}` query key |

### 6.6 DB Table Conventions

| Entity | Table | Key Notes |
|--------|-------|-----------|
| `TreeNode` | `canvas_treenode` | Index on `(TreeId)`. `CanvasCoordinates` via `OwnsOne()`. |
| `TreeNodeMember` | `canvas_treenode_member` | Composite PK `(TreeNodeId, FamilyMemberId)`. No uniqueness constraint on `FamilyMemberId` alone. |
| `TreeEdge` | `canvas_treeedge` | Index on `(TreeId)`. FKs to `canvas_treenode` (cascade). |

---

## 6.5 Trees Architecture

The Trees module manages the core family tree instances and Role-Based Access Control (`TreeRbac`).
* **Accessible Trees**: `GET /api/trees` fetches all trees the current authenticated user has access to by querying `TreeRbac` for the `UserId` and mapping to the corresponding `Tree` entities via `GetAccessibleTreesQuery`.

---

## 7. Error Handling

### 7.1 Layer Responsibilities

| Layer | Mechanism | Rule |
|---|---|---|
| **Domain** | `Result<T>` / `Result` | Never throws for business rule violations |
| **Application** | Returns `Result<T>` | Catches infra exceptions only if a meaningful domain error can be produced |
| **Infrastructure** | May throw | EF Core, network, external API failures are exceptional |
| **API** | `ApiControllerBase` | Maps `ErrorType` to corresponding HTTP status codes |

### 7.2 Error Type Taxonomy

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

## 8. Authentication Design

**Current Implementation**: Google OAuth with ASP.NET Core Cookie Authentication.
- `AddGoogleOpenIdConnect` handles the OAuth flow.
- Session persisted as an encrypted cookie.
- `AuthController` orchestrates user upsert via `ProcessExternalLoginCommandHandler`.
- The `IAuthService` provides a thin wrapper for cookie sign-in via `SignInAsync`.

---

## 9. Data Access

**Direct DbContext Pattern**: All data access is performed directly via `IApplicationDbContext` rather than using the Repository pattern. This provides full access to LINQ, reduces boilerplate, simplifies DTO projections, and is used consistently across Trees, Users, and Roster modules.

---

## 10. API & Documentation

- **Swagger/OpenAPI**: Configured in `Program.cs` with an OpenID Connect security definition. Swagger UI is available at `/swagger` during development.
- **ApiControllerBase**: All controllers inherit from this base class, which provides helper methods for mapping `Result` to the appropriate `IActionResult` based on the `ErrorType`.

---

## 11. Decision Records

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

### Decision — 2026-08 (Phase 4) — Canvas Caching: On-the-fly Querying + Frontend TanStack Query
**Decision:** No server-side materialized views or Redis caching for the Canvas in Phase 4. The `GetCanvasQuery` runs fresh against the DB on each request. The frontend uses TanStack Query for caching and invalidation.
**Context:** The `VisibilityMediator` applies per-requester masking (Owner sees all, Public sees "Anonymous"), making the response inherently user-role-specific. This makes a single shared server-side cache infeasible without complex role-partitioning.
**Rationale:** On-the-fly querying is simpler to implement, correct by default, and sufficient for Phase 4 scale. Server-side Redis caching (partitioned by role) is deferred to Phase 6 where it already belongs in the plan.
**Impact:** `GetCanvasQueryHandler` runs a DB query on each request. Acceptable for Phase 4 tree sizes.
**Review:** Revisit when Phase 6 Redis caching is implemented.

### Decision — 2026-08 (Phase 4) — Canvas Editing: Manual Save + 5-Minute Auto-Save
**Decision:** Canvas coordinate changes are NOT persisted on drag. The user must click "Save Layout" explicitly. A background auto-save fires every 5 minutes if there are unsaved changes, showing a "Saving..." toast.
**Context:** React Flow updates coordinates dozens of times per second during drag. Persisting every event would hammer the backend.
**Rationale:** Manual save prevents accidental layout changes and keeps the backend load predictable. The 5-minute auto-save prevents data loss without requiring continuous user action.
**Impact:** The frontend must track "dirty" state on the canvas and provide a Save button. `UpdateCanvasCommand` accepts a bulk list of `(NodeId, X, Y)` tuples.
**Review:** Consider debounced autosave instead of interval-based if user feedback indicates 5 minutes is too long.

### Decision — 2026-08 (Phase 4) — FamilyMember Duplication on Canvas Allowed
**Decision:** A single `FamilyMember` may appear on more than one `TreeNode` (no uniqueness constraint on `TreeNodeMember.FamilyMemberId`).
**Context:** Complex family lineages (e.g., half-siblings, adoptions, multiple marriages) may require placing the same biological person visually in multiple node contexts.
**Rationale:** Strict 1:1 node-to-member mapping would artificially constrain valid genealogical representations.
**Impact:** No unique index on `(FamilyMemberId)` in `canvas_treenode_member`. Validation checks only that the member belongs to the correct tree.
**Review:** If data integrity issues arise, consider a soft uniqueness guard (application-level warning, not hard constraint).
