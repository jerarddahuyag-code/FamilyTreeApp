# FamilyTreeApp — Requirements

> **Status:** Living document. Updated at the end of each phase.
> **Notation:** [EARS](https://alistairmavin.com/ears/) — Easy Approach to Requirements Syntax
> **Last updated:** Phase 0 / Phase 1 gap-filling pass

---

## Confirmed Architectural Decisions

| Concern | Decision | Rationale |
|---|---|---|
| **Project naming** | `FamilyTreeApp.*` throughout | Existing codebase convention |
| **Authentication** | Google OAuth with ASP.NET Core cookie authentication | Simplifies token management; no custom JWT issuance |
| **Input validation** | FluentValidation via `ValidationPipelineBehavior` | Declarative, testable, auto-registered via Scrutor |
| **Domain invariants** | Guard clauses returning `Result<T>` / `Result` | No exceptions for expected failures; caller always handles |
| **CQRS pipeline** | Named *behaviors* following `ValidationPipelineBehavior` pattern | Consistent structure, primary constructor, Scrutor-decorated |
| **Handler injection** | Direct `[FromServices]` injection in controllers | No mediator/dispatcher; explicit dependencies |
| **Error handling** | `Result<T>` for domain failures; exceptions for infra faults | Clear separation; global handler maps to RFC 7807 |
| **Caching** | `IDistributedCache` abstraction; `MemoryDistributedCache` initially | Redis swap-in (Phase 6) requires no application-layer changes |
| **Async processing** | Synchronous-first | Deferred to Phase 7 |
| **pgvector** | `pgvector/pgvector:pg18` Docker image (extension pre-installed) | Enables Phase 7 vector search without re-provisioning |

---

## Phase 0 — Standards & CI

### R-0.1 Roslyn Analyzer Enforcement
**THE SYSTEM SHALL** enforce Roslyn analyzer rules defined in `.globalconfig` at build time, treating
nullable reference type violations and unnecessary `using` directives as build errors.

### R-0.2 CI Pipeline — Formatting
**WHEN** a commit is pushed to `main` or a pull request targets `main`,
**THE SYSTEM SHALL** run `dotnet format --verify-no-changes` and fail the build
if any formatting violation is detected.

### R-0.3 CI Pipeline — Build
**WHEN** a commit is pushed to `main` or a pull request targets `main`,
**THE SYSTEM SHALL** compile all projects with `--warnaserror` and fail the pipeline
if any compiler warning is emitted.

### R-0.4 CI Pipeline — Tests
**WHEN** a commit is pushed to `main` or a pull request targets `main`,
**THE SYSTEM SHALL** execute all unit tests and fail the pipeline if any test fails
or if no tests are discovered.

### R-0.5 Local Infrastructure — Redis
**THE SYSTEM SHALL** provide a `redis:7-alpine` service in `docker-compose.yml`
alongside the existing PostgreSQL service, accessible on host port `6379`,
so that Phase 6 caching work requires zero Docker Compose changes.

### R-0.6 Test Project
**THE SYSTEM SHALL** provide an xUnit test project (`FamilyTreeApp.Tests`) that
references all application layers and contains at minimum: domain entity tests,
value object tests, and command handler tests discoverable by `dotnet test`.

---

## Phase 1 — Architecture Foundation (Gap Filling)

### R-1.1 Logging Behavior
**WHEN** any command is dispatched through `ICommandHandler<TCommand, TResult>`,
**THE SYSTEM SHALL** log the command type name, elapsed execution time in milliseconds,
and whether the result was a success or failure using structured logging (Serilog).

### R-1.2 Logging Behavior — Failure Detail
**WHEN** a command handler returns `Result.IsFailure`,
**THE SYSTEM SHALL** log the `Error.Code` and `Error.Description` at `Warning` level
without logging the request payload (to avoid PII leakage).

### R-1.3 Transaction Behavior — Conditional Wrapping
**WHEN** a command implements `ITransactionalCommand` and is dispatched through
`ICommandHandler<TCommand, TResult>`, **THE SYSTEM SHALL** wrap the handler execution
inside a database transaction that is committed when `Result.IsSuccess` is `true`
and rolled back when `Result.IsFailure` is `true` or any exception is thrown.

### R-1.4 Transaction Behavior — Pass-Through
**WHEN** a command does NOT implement `ITransactionalCommand`,
**THE SYSTEM SHALL** pass the call directly to the inner handler
without opening a database transaction.

### R-1.5 Notification Publisher Interface
**THE SYSTEM SHALL** define `INotificationPublisher` in the Application layer so that
domain handlers can publish events without a compile-time dependency on any concrete
message bus or in-process dispatcher (concrete implementation deferred to Phase 6).

### R-1.6 Notification Handler Interface
**THE SYSTEM SHALL** define a generic `INotificationHandler<T>` in the Application layer
to establish the contract for all future domain event handlers
(concrete implementations deferred to Phase 6).

### R-1.7 Transactional Command Marker
**THE SYSTEM SHALL** define `ITransactionalCommand` as an empty marker interface
in the Application layer so that commands requiring transaction wrapping are
explicitly opt-in and declared at compile time.

### R-1.8 Behavior Pipeline Order
**THE SYSTEM SHALL** apply behaviors in the following execution order for every command:

```
ValidationPipelineBehavior → LoggingBehavior → TransactionBehavior → Handler
```

**IF** validation fails, **THE SYSTEM SHALL NOT** invoke `LoggingBehavior`
or `TransactionBehavior`.

---

## Phase 2 — Trees + RBAC (Gap Filling)

### R-2.1 Tree RBAC Authorization Policies
**WHEN** a request reaches a tree-scoped endpoint,
**THE SYSTEM SHALL** resolve the requesting user's `TreeRole` from `TreeRbac`
and enforce hierarchical access: `Owner > Admin > Member`.

### R-2.2 RBAC Lookup Caching
**WHEN** a tree RBAC lookup is performed,
**THE SYSTEM SHALL** cache the result in `IDistributedCache` with a 5-minute TTL
to reduce database round-trips on repeated requests.

---

## Phase 3 — Roster (Pending)

### R-3.1 FamilyMember Creation
**WHEN** an authorized tree admin creates a family member,
**THE SYSTEM SHALL** persist a `FamilyMember` entity with `VisibilityStatus`
defaulting to `Hidden`.

### R-3.2 Visibility State Machine — Valid Transitions
**THE SYSTEM SHALL** permit only the following visibility transitions:

| From | To | Trigger |
|---|---|---|
| `Hidden` | `PendingApproval` | Member requests visibility |
| `PendingApproval` | `Visible` | Admin approves |
| `PendingApproval` | `Hidden` | Admin rejects |
| `Visible` | `Hidden` | Admin revokes |

### R-3.3 Visibility State Machine — Invalid Transitions
**IF** a requested visibility transition is not in the permitted set,
**THE SYSTEM SHALL** return `Result.Failure(DomainErrors.Visibility.InvalidTransition)`
without mutating the entity state.

### R-3.4 Read-Through Profile Merge
**WHEN** a family member has a non-null `ClaimedByUserId`,
**THE SYSTEM SHALL** merge the linked `User.ProfileInfo` fields over
the `FamilyMember.ProfileInfo` fields, using the User's value where non-null
and falling back to the FamilyMember's value for any null User fields.

### R-3.5 Anonymous Masking
**WHEN** a family member's `VisibilityStatus` is not `Visible` AND
the requesting user is not an authorized tree admin,
**THE SYSTEM SHALL** return an anonymised placeholder in place of
the member's personal data.

### R-3.6 Relationship Same-Tree Constraint
**WHEN** a relationship is created between two family members,
**THE SYSTEM SHALL** verify both members belong to the same tree
and return `Result.Failure(DomainErrors.Roster.MemberTreeMismatch)`
if they do not.

---

## Phase 4 — Canvas (Pending)

### R-4.1 Three-Layer Canvas Model
**THE SYSTEM SHALL** maintain a three-layer canvas model:
- **Biological layer:** `FamilyMember` + `FamilyMemberRelationship`
- **Visual layer:** `TreeNode` + `TreeNodeMember` (join) + `TreeEdge`
- Node types: `Single`, `Partner`, `MultiPerson`

### R-4.2 Coordinate Updates
**WHEN** a tree owner or admin submits bulk coordinate updates for canvas nodes,
**THE SYSTEM SHALL** persist all position changes atomically
(entire batch succeeds or entire batch is rolled back).

---

## Phase 5 — Authentication Hardening (Pending)

### R-5.1 Authenticated-Only Endpoints
**WHEN** Phase 5 begins, **THE SYSTEM SHALL** enforce `[Authorize]` on all
endpoints that were previously `[AllowAnonymous]` during development,
requiring a valid Google OAuth session cookie.

---

## Phase 6 — API Hardening (Pending)

### R-6.1 Rate Limiting
**THE SYSTEM SHALL** apply rate limiting to all public endpoints
using ASP.NET Core's built-in `RateLimiter` middleware.

### R-6.2 Redis Cache Swap
**WHEN** Redis is available in the environment,
**THE SYSTEM SHALL** replace `MemoryDistributedCache` with
`StackExchange.Redis` via `IDistributedCache` — no application-layer
code changes required.

### R-6.3 Notification Publisher — Concrete Implementation
**THE SYSTEM SHALL** provide a concrete `INotificationPublisher` implementation
that fan-outs notifications to all registered `INotificationHandler<T>` instances.

---

## Edge Cases & Failure Matrix

| Scenario | Expected Behaviour |
|---|---|
| Command with no registered validator | `ValidationPipelineBehavior` passes through (no validators = no failure) |
| `ITransactionalCommand` handler throws exception | `TransactionBehavior` rolls back transaction and re-throws |
| `ITransactionalCommand` handler returns `Result.Failure` | `TransactionBehavior` rolls back; `LoggingBehavior` logs Warning |
| Family member claimed by a deleted user | `ClaimedByUserId` FK is `SET NULL`; member falls back to its own `ProfileInfo` |
| Visibility transition from `Visible` to `PendingApproval` | Domain returns `Result.Failure(DomainErrors.Visibility.InvalidTransition)` |
| Relationship between members of different trees | Application returns `Result.Failure(DomainErrors.Roster.MemberTreeMismatch)` |
| CI push with unformatted code | Pipeline fails at format-check step before build or tests run |
