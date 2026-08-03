# FamilyTreeApp — Implementation Tasks

> **Legend:** 🔴 Not started | 🟡 In progress | 🟢 Complete | ⏭ Deferred
> **Last updated:** Phase 0 / Phase 1 gap-filling pass

---

## Phase 0 — Standards & CI

---

### TASK-0.1 — Add `.globalconfig` 🟢

**File:** `.globalconfig` (solution root)

**What:** Roslyn analyzer rule severity configuration applied solution-wide.

**Acceptance Criteria:**
- `dotnet_nullable` context enabled as error
- `IDE0005` (remove unnecessary using) severity = error
- `CA1062` (validate arguments of public methods) severity = warning
- `CA2007` (do not directly await a Task without ConfigureAwait) severity = warning
- `CA1848` (use LoggerMessage delegates for performance) severity = suggestion
- `CA1822` (mark members as static) severity = suggestion
- `dotnet build --warnaserror` passes from solution root with these rules active

**Verify:**
```powershell
dotnet build --warnaserror
```

---

### TASK-0.2 — Add Redis to `docker-compose.yml` 🟢

**File:** `docker-compose.yml` (solution root)

**What:** Add a `redis` service and `redis_data` volume to the existing file.
Do NOT modify existing `database` or `backend` services.

**Change:** Insert under `services`:
```yaml
redis:
  image: redis:7-alpine
  container_name: family_tree_redis
  restart: always
  ports:
	- "6379:6379"
  volumes:
	- redis_data:/data
  networks:
	- mynet
```
Add `redis_data:` under the `volumes:` block alongside `postgres_data` and `minio_data`.

**Acceptance Criteria:**
- `docker compose up -d` starts all services without error
- `docker compose ps` shows `database`, `backend`, and `redis` as running

**Verify:**
```powershell
docker compose up -d
docker compose ps
docker compose down
```

---

### TASK-0.3 — Create GitHub Actions CI workflow 🟢

**File:** `.github/workflows/ci.yml`

**What:** Automated pipeline that validates formatting, build, and tests on every push/PR.

**Steps in workflow:**
1. `actions/checkout@v4`
2. `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`
3. `dotnet restore`
4. `dotnet format --verify-no-changes --verbosity diagnostic`
5. `dotnet build --no-restore --warnaserror`
6. `dotnet test --no-build --verbosity normal`

**Trigger:** `push` to `main`; `pull_request` targeting `main`

**Acceptance Criteria:**
- Pipeline is green on a clean push
- Pipeline fails if code has formatting violations
- Pipeline fails if any test fails

**Verify:** Push to main branch; check GitHub Actions tab.

---

### TASK-0.4 — Create test project `FamilyTreeApp.Tests` 🟢

**File:** `FamilyTreeApp/FamilyTreeApp.Tests/FamilyTreeApp.Tests.csproj`

**What:** xUnit test project targeting .NET 10; add to solution.

**NuGet packages (latest stable):**
- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `coverlet.collector`
- `FluentAssertions`
- `NSubstitute`

**Project references:**
- `FamilyTreeApp.Domain`
- `FamilyTreeApp.Application`
- `FamilyTreeApp.Infrastructure`

**Solution:** Register in `FamilyTreeApp.slnx`

**Acceptance Criteria:**
- `dotnet build` compiles with no errors
- `dotnet test` discovers tests (even if 0 initially)
- Project appears in Visual Studio Solution Explorer

**Verify:**
```powershell
dotnet build FamilyTreeApp/FamilyTreeApp.Tests/FamilyTreeApp.Tests.csproj
dotnet test FamilyTreeApp/FamilyTreeApp.Tests/FamilyTreeApp.Tests.csproj
```

---

**What:** Automated pipeline that validates formatting, build, and tests on every push/PR.

**Steps in workflow:**
1. `actions/checkout@v4`
2. `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`
3. `dotnet restore`
4. `dotnet format --verify-no-changes --verbosity diagnostic`
5. `dotnet build --no-restore --warnaserror`
6. `dotnet test --no-build --verbosity normal`

**Trigger:** `push` to `main`; `pull_request` targeting `main`

**Acceptance Criteria:**
- Pipeline is green on a clean push
- Pipeline fails if code has formatting violations
- Pipeline fails if any test fails

**Verify:** Push to main branch; check GitHub Actions tab.

---

### TASK-0.5 — Write domain and handler unit tests 🔴

**Depends on:** TASK-0.4

**Files:**
```
FamilyTreeApp.Tests/Unit/Domain/UserTests.cs
FamilyTreeApp.Tests/Unit/Domain/TreeTests.cs
FamilyTreeApp.Tests/Unit/Domain/TreeRbacTests.cs
FamilyTreeApp.Tests/Unit/Application/Trees/CreateTreeCommandHandlerTests.cs
```

**`UserTests.cs` scenarios:**
| Test name | Input | Expected |
|---|---|---|
| `Create_WithValidEmailAndProfile_ReturnsSuccess` | valid email + ProfileInfo | `Result.IsSuccess == true` |
| `Create_WithEmptyEmail_ReturnsFailure` | `""` email | `Error == DomainErrors.UserErrors.InvalidEmail` |
| `Create_WithWhitespaceEmail_ReturnsFailure` | `"   "` email | `Error == DomainErrors.UserErrors.InvalidEmail` |
| `Create_WithMalformedEmail_ReturnsFailure` | `"notanemail"` | `Error == DomainErrors.UserErrors.InvalidEmail` |
| `Create_WithNullProfile_ReturnsFailure` | null ProfileInfo | `Error == DomainErrors.UserErrors.InvalidProfile` |

**`TreeTests.cs` scenarios:**
| Test name | Input | Expected |
|---|---|---|
| `Create_WithValidName_ReturnsSuccess` | valid name + description | `Result.IsSuccess == true` |
| `Create_WithEmptyName_ReturnsFailure` | `""` name | `Error == DomainErrors.TreeErrors.InvalidTreeName` |
| `Create_WithWhitespaceName_ReturnsFailure` | `"   "` name | `Error == DomainErrors.TreeErrors.InvalidTreeName` |
| `UpdateDetails_WithEmptyName_ReturnsFailure` | existing tree, empty name | `Result.IsFailure == true` |
| `UpdateDetails_WithValidName_ReturnsSuccess` | existing tree, valid name | `Result.IsSuccess == true` |

**`TreeRbacTests.cs` scenarios:**
| Test name | Input | Expected |
|---|---|---|
| `Create_WithOwnerRole_ReturnsSuccess` | valid userId + treeId + Owner | `Result.IsSuccess == true` |
| `Create_WithMemberRole_ReturnsSuccess` | valid userId + treeId + Member | `Result.IsSuccess == true` |

**`CreateTreeCommandHandlerTests.cs` scenarios:**
| Test name | Setup | Expected |
|---|---|---|
| `HandleAsync_WithValidCommand_ReturnsTreeGuid` | repository mock; no duplicate | `Result.IsSuccess == true`, value is a non-empty Guid |
| `HandleAsync_WithEmptyName_ReturnsFailure` | any setup | `Result.IsFailure == true` (domain guard fails before repo) |

**Conventions:**
- Use NSubstitute for repository mocks
- Assert with FluentAssertions
- No `async void` tests — use `async Task`

**Verify:**
```powershell
dotnet test --verbosity normal
```
All tests must be green.

---

## Phase 1 — Architecture Foundation (Gap Filling)

---

### TASK-1.1 — Create `ITransactionalCommand` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Application/Common/Interfaces/ITransactionalCommand.cs`

**What:** Empty marker interface. Commands implement this to opt-in to automatic
transaction wrapping by `TransactionBehavior`.

```csharp
namespace FamilyTreeApp.Application.Common.Interfaces;

/// <summary>
/// Marker interface. Commands implementing this are automatically wrapped
/// in a database transaction by <see cref="TransactionBehavior{TRequest,TResponse}"/>.
/// </summary>
public interface ITransactionalCommand { }
```

**Acceptance Criteria:**
- Interface is empty (marker only)
- Namespace: `FamilyTreeApp.Application.Common.Interfaces`
- Located in Application layer (not Domain)

---

### TASK-1.2 — Create `LoggingBehavior` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Application/Common/Behaviors/LoggingBehavior.cs`

**Depends on:** TASK-1.1 (same layer, same pass)

**Pattern:** Identical structure to `ValidationPipelineBehavior` — primary constructor,
implements `ICommandHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>`.

**Dependencies (constructor):**
1. `ICommandHandler<TRequest, TResponse> innerHandler`
2. `ILogger<LoggingBehavior<TRequest, TResponse>> logger`

**Logic:**
1. `var sw = Stopwatch.StartNew()`
2. `var result = await innerHandler.HandleAsync(command, cancellationToken)`
3. `sw.Stop()`
4. If `result.IsSuccess`: log `Information` — `"Command {CommandType} succeeded in {ElapsedMs}ms"`
5. If `result.IsFailure`: log `Warning` — `"Command {CommandType} failed in {ElapsedMs}ms: [{ErrorCode}] {ErrorDescription}"`
6. Return result

**Acceptance Criteria:**
- Does NOT log request payload
- Does NOT catch exceptions (they propagate to caller)
- `{CommandType}` is `typeof(TRequest).Name`
- Unit testable via NSubstitute mock for `innerHandler` and `ILogger`

---

### TASK-1.3 — Create `TransactionBehavior` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Application/Common/Behaviors/TransactionBehavior.cs`

**Depends on:** TASK-1.1

**Pattern:** Identical structure to `ValidationPipelineBehavior`.

**Dependencies (constructor):**
1. `ICommandHandler<TRequest, TResponse> innerHandler`
2. `IApplicationDbContext dbContext`

**Logic:**
```
if TRequest does NOT implement ITransactionalCommand:
	return await innerHandler.HandleAsync(command, cancellationToken)

else:
	await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken)
	try:
		var result = await innerHandler.HandleAsync(command, cancellationToken)
		if result.IsSuccess:
			await tx.CommitAsync(cancellationToken)
		else:
			await tx.RollbackAsync(cancellationToken)
		return result
	catch:
		await tx.RollbackAsync(cancellationToken)
		throw
```

**Acceptance Criteria:**
- Non-transactional command: `BeginTransactionAsync` is never called
- Transactional + success: `CommitAsync` called once, `RollbackAsync` never called
- Transactional + failure result: `RollbackAsync` called once, `CommitAsync` never called
- Transactional + exception: `RollbackAsync` called once, exception re-thrown unchanged

---

### TASK-1.4 — Create `INotificationPublisher` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Application/Common/Interfaces/INotificationPublisher.cs`

**What:** Interface for publishing domain events. No concrete implementation yet.

```csharp
namespace FamilyTreeApp.Application.Common.Interfaces;

public interface INotificationPublisher
{
	Task PublishAsync<T>(T notification, CancellationToken cancellationToken = default)
		where T : notnull;
}
```

---

### TASK-1.5 — Create `INotificationHandler` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Application/Common/Interfaces/INotificationHandler.cs`

**What:** Generic contract for domain event handlers. No concrete handlers yet.

```csharp
namespace FamilyTreeApp.Application.Common.Interfaces;

public interface INotificationHandler<in T> where T : notnull
{
	Task HandleAsync(T notification, CancellationToken cancellationToken = default);
}
```

---

### TASK-1.6 — Register behaviors in `DependencyInjection.cs` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Application/DependencyInjection.cs`

**Depends on:** TASK-1.2, TASK-1.3

**What:** Add `Decorate` calls for `TransactionBehavior` and `LoggingBehavior`
**before** the existing `ValidationPipelineBehavior` decoration, so that
`ValidationPipelineBehavior` remains the outermost layer.

**Final registration order:**
```csharp
// 1. Scan and register raw handlers (unchanged)
services.Scan(...)   // ICommandHandler<,>
services.Scan(...)   // IQueryHandler<,>

// 2. Register FluentValidation validators (unchanged)
services.AddValidatorsFromAssembly(assembly);

// 3. Behaviors — registered innermost-first (LIFO = last registered = outermost)
services.Decorate(typeof(ICommandHandler<,>), typeof(TransactionBehavior<,>));   // NEW
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingBehavior<,>));        // NEW
services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationPipelineBehavior<,>));  // EXISTING (moved last)
```

**Acceptance Criteria:**
- `dotnet build --warnaserror` passes
- `dotnet test` passes (all existing tests still green)
- Serilog console output shows `LoggingBehavior` log entries when a command is dispatched manually

---

### TASK-1.7 — Write behavior unit tests 🔴

**Depends on:** TASK-0.4, TASK-1.2, TASK-1.3

**Files:**
```
FamilyTreeApp.Tests/Unit/Application/Behaviors/LoggingBehaviorTests.cs
FamilyTreeApp.Tests/Unit/Application/Behaviors/TransactionBehaviorTests.cs
```

**`LoggingBehaviorTests.cs` scenarios:**
| Test name | Setup | Expected |
|---|---|---|
| `HandleAsync_OnSuccess_LogsAtInformationLevel` | inner returns success | `logger` received 1 `Information` call containing command type name |
| `HandleAsync_OnFailure_LogsAtWarningLevel` | inner returns failure | `logger` received 1 `Warning` call containing error code |
| `HandleAsync_NeverLogsRequestPayload` | any | logged message does not contain serialized request properties |

**`TransactionBehaviorTests.cs` scenarios:**
| Test name | Setup | Expected |
|---|---|---|
| `HandleAsync_NonTransactionalCommand_NoTransactionOpened` | command without marker | `BeginTransactionAsync` never called |
| `HandleAsync_TransactionalCommand_Success_CommitsTransaction` | command with marker; inner succeeds | `CommitAsync` called once |
| `HandleAsync_TransactionalCommand_FailureResult_RollsBack` | command with marker; inner returns failure | `RollbackAsync` called once, `CommitAsync` never called |
| `HandleAsync_TransactionalCommand_ThrowsException_RollsBackAndRethrows` | command with marker; inner throws | `RollbackAsync` called once, exception propagates |

---

## Phase 2 — Trees + RBAC (Gap Filling)

---

### TASK-2.1 — Implement RBAC authorization policies 🔴

**Files:**
```
FamilyTreeApp.Api/Authorization/TreeRoles.cs
FamilyTreeApp.Api/Authorization/TreeOwnerRequirement.cs
FamilyTreeApp.Api/Authorization/TreeAdminRequirement.cs
FamilyTreeApp.Api/Authorization/TreeMemberRequirement.cs
FamilyTreeApp.Api/Authorization/TreeAuthorizationHandler.cs
```

**Logic in `TreeAuthorizationHandler`:**
1. Extract `tree_id` from route values
2. Extract authenticated `UserId` from claims
3. Query `IUserRepository` (or `IApplicationDbContext`) for `TreeRbac` by `(TreeId, UserId)`
4. Evaluate hierarchically: `Owner` satisfies Owner/Admin/Member requirements; `Admin` satisfies Admin/Member; `Member` satisfies Member only
5. Cache lookup in `IDistributedCache` with 5-minute TTL (key: `"rbac:{treeId}:{userId}"`)

**Register in `Program.cs`:**
```csharp
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("TreeOwner",  p => p.AddRequirements(new TreeOwnerRequirement()));
	options.AddPolicy("TreeAdmin",  p => p.AddRequirements(new TreeAdminRequirement()));
	options.AddPolicy("TreeMember", p => p.AddRequirements(new TreeMemberRequirement()));
});
```

**Acceptance Criteria:**
- Owner can access Owner-only, Admin-only, and Member-only endpoints
- Admin cannot access Owner-only endpoints
- Member cannot access Admin-only or Owner-only endpoints
- Unauthenticated returns 401; insufficient role returns 403

---

### TASK-2.2 — Add RBAC caching 🔴

**Depends on:** TASK-2.1

**What:** In `TreeAuthorizationHandler`, wrap the `TreeRbac` DB query in an
`IDistributedCache` read-through with a 5-minute TTL.

**Acceptance Criteria:**
- Second call for same `(treeId, userId)` within TTL does not hit database
- Cache is invalidated or TTL expires naturally (active invalidation deferred to Phase 6)

---

## Phase 3 — Roster

---

### TASK-3.1 — Domain: `FamilyMember` entity 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Domain/Roster/Entities/FamilyMember.cs`

**Properties:** `Guid Id`, `Guid TreeId`, `Guid? ClaimedByUserId`, `ProfileInfo PersonalInfo`,
`VisibilityStatus VisibilityStatus` (default `Hidden`), `DateTime CreatedAt`, `DateTime UpdatedAt`

**Domain methods:**
- `Result CanTransitionTo(VisibilityStatus target)` — validates state machine rules (see R-3.2)
- `Result TransitionTo(VisibilityStatus target)` — calls `CanTransitionTo`, mutates state on success
- `static Result<FamilyMember> Create(Guid treeId, ProfileInfo personalInfo)` — factory

---

### TASK-3.2 — Domain: `FamilyMemberRelationship` entity 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Domain/Roster/Entities/FamilyMemberRelationship.cs`

**Properties:** `Guid Id`, `Guid TreeId`, `Guid BaseMemberId`, `Guid TargetMemberId`,
`RelationshipType RelationshipType`, `DateTime CreatedAt`, `DateTime UpdatedAt`

**Constraint:** Composite unique `(BaseMemberId, TargetMemberId, RelationshipType)` — enforced at DB level.

---

### TASK-3.3 — Domain: Enums 🔴

**Files:**
- `FamilyTreeApp/FamilyTreeApp.Domain/Roster/Enums/VisibilityStatus.cs` — `Hidden`, `PendingApproval`, `Visible`
- `FamilyTreeApp/FamilyTreeApp.Domain/Roster/Enums/RelationshipType.cs` — `Parent`, `Spouse`, `Sibling`

---

### TASK-3.4 — Domain: Repository interfaces 🔴

**Files:**
- `FamilyTreeApp/FamilyTreeApp.Domain/Roster/Interfaces/IFamilyMemberRepository.cs`
- `FamilyTreeApp/FamilyTreeApp.Domain/Roster/Interfaces/IFamilyMemberRelationshipRepository.cs`

---

### TASK-3.5 — Domain: `DomainErrors` — Roster entries 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Domain/Common/Errors/DomainErrors.cs`

**Add:**
```csharp
public static class Visibility
{
	public static readonly Error InvalidTransition = new("Visibility.InvalidTransition", "The requested visibility transition is not permitted.");
}

public static class Roster
{
	public static readonly Error MemberTreeMismatch = new("Roster.MemberTreeMismatch", "Both family members must belong to the same tree.");
	public static readonly Error MemberNotFound     = new("Roster.MemberNotFound",     "The specified family member does not exist.");
}
```

---

### TASK-3.6 — Application: Roster commands 🔴

**Files:**
```
FamilyTreeApp.Application/Roster/Commands/AddFamilyMember/AddFamilyMemberCommand.cs
FamilyTreeApp.Application/Roster/Commands/AddFamilyMember/AddFamilyMemberCommandHandler.cs
FamilyTreeApp.Application/Roster/Commands/AddFamilyMember/AddFamilyMemberCommandValidator.cs
FamilyTreeApp.Application/Roster/Commands/UpdateFamilyMember/...
FamilyTreeApp.Application/Roster/Commands/RequestVisibility/...
FamilyTreeApp.Application/Roster/Commands/AddRelationship/...
FamilyTreeApp.Application/Roster/Commands/RemoveRelationship/...
```

`AddFamilyMemberCommand` implements `ITransactionalCommand`.

---

### TASK-3.7 — Application: Roster queries 🔴

**Files:**
```
FamilyTreeApp.Application/Roster/Queries/GetFamilyMembers/GetFamilyMembersQuery.cs
FamilyTreeApp.Application/Roster/Queries/GetFamilyMembers/GetFamilyMembersQueryHandler.cs
FamilyTreeApp.Application/Roster/Queries/GetRelationships/...
FamilyTreeApp.Application/Roster/DTOs/FamilyMemberDto.cs
FamilyTreeApp.Application/Roster/DTOs/RelationshipDto.cs
```

`GetFamilyMembersQueryHandler` implements Read-Through merge logic (R-3.4) and
anonymous masking (R-3.5).

---

### TASK-3.8 — Infrastructure: EF Core configurations + repositories 🔴

**Files:**
```
FamilyTreeApp.Infrastructure/Persistence/Model Configurations/FamilyMemberConfiguration.cs
FamilyTreeApp.Infrastructure/Persistence/Model Configurations/FamilyMemberRelationshipConfiguration.cs
FamilyTreeApp.Infrastructure/Persistence/Repositories/FamilyMemberRepository.cs
FamilyTreeApp.Infrastructure/Persistence/Repositories/FamilyMemberRelationshipRepository.cs
```

**Migration:** `dotnet ef migrations add AddRoster --project FamilyTreeApp.Infrastructure --startup-project FamilyTreeApp.Api`

**Table names:** `roster_family_member`, `roster_family_member_relationship`

---

### TASK-3.9 — API: `RosterController` 🔴

**File:** `FamilyTreeApp/FamilyTreeApp.Api/Controllers/RosterController.cs`

**Endpoints:**
| Method | Route | Policy |
|---|---|---|
| `GET` | `/api/trees/{treeId}/members` | `TreeMember` |
| `POST` | `/api/trees/{treeId}/members` | `TreeAdmin` |
| `PUT` | `/api/trees/{treeId}/members/{memberId}` | `TreeAdmin` |
| `DELETE` | `/api/trees/{treeId}/members/{memberId}` | `TreeAdmin` |
| `POST` | `/api/trees/{treeId}/members/{memberId}/visibility` | `TreeAdmin` |
| `GET` | `/api/trees/{treeId}/members/{memberId}/relationships` | `TreeMember` |
| `POST` | `/api/trees/{treeId}/members/{memberId}/relationships` | `TreeAdmin` |
| `DELETE` | `/api/trees/{treeId}/members/{memberId}/relationships/{relationshipId}` | `TreeAdmin` |

---

### TASK-3.10 — Tests: Roster unit tests 🔴

**Files:**
```
FamilyTreeApp.Tests/Unit/Domain/FamilyMemberTests.cs
FamilyTreeApp.Tests/Unit/Domain/FamilyMemberVisibilityTests.cs
FamilyTreeApp.Tests/Unit/Application/Roster/AddFamilyMemberCommandHandlerTests.cs
FamilyTreeApp.Tests/Unit/Application/Roster/GetFamilyMembersQueryHandlerTests.cs
```

**Key scenarios:** all valid/invalid visibility transitions, Read-Through merge, anonymous masking,
same-tree constraint for relationships.

---

## Verification Gates

### Phase 0 Gate
```powershell
dotnet format --verify-no-changes
dotnet build --warnaserror
dotnet test
docker compose up -d
docker compose ps   # database, backend, redis all healthy
docker compose down
```

### Phase 1 Gate
```powershell
dotnet build --warnaserror
dotnet test
# Manual: POST /api/trees — observe Serilog console showing LoggingBehavior entries
```

### Phase 2 Gate
```powershell
dotnet test
# Manual: verify 401/403/200 responses across RBAC policy combinations
```

### Phase 3 Gate
```powershell
dotnet ef migrations list --project FamilyTreeApp/FamilyTreeApp.Infrastructure --startup-project FamilyTreeApp/FamilyTreeApp.Api
dotnet build --warnaserror
dotnet test
# Manual: full roster CRUD via REST client; verify visibility state machine transitions
```
