# FamilyTreeApp — Implementation Tasks

> **Legend:** 🔴 Not started | 🟡 In progress | 🟢 Complete | ⏭ Deferred
> **Last updated:** Phase 3 Roster complete, preparing for Phase 4

---

## Phase 0 — Standards & CI

### TASK-0.1 — Add `.globalconfig` 🟢
### TASK-0.2 — Add Redis to `docker-compose.yml` 🟢
### TASK-0.3 — Create GitHub Actions CI workflow 🟢
### TASK-0.4 — Create test project `FamilyTreeApp.Tests` 🟢
### TASK-0.5 — Write domain and handler unit tests 🟢

---

## Phase 1 — Architecture Foundation (Gap Filling)

### TASK-1.1 — Create `ITransactionalCommand` 🟢
### TASK-1.2 — Create `LoggingBehavior` 🟢
### TASK-1.3 — Create `TransactionBehavior` 🟢
### TASK-1.4 — Create `INotificationPublisher` 🟢
### TASK-1.5 — Create `INotificationHandler` 🟢
### TASK-1.6 — Register behaviors in `DependencyInjection.cs` 🟢
### TASK-1.7 — Write behavior unit tests 🟢

---

## Phase 2 — Trees + RBAC (Gap Filling)

### TASK-2.1 — Implement RBAC authorization policies 🟢
### TASK-2.2 — Add RBAC caching 🟢

---

## Phase 3 — Roster

### TASK-3.1 — Domain: `FamilyMember` entity 🟢
### TASK-3.2 — Domain: `FamilyMemberRelationship` entity 🟢
### TASK-3.3 — Domain: Enums 🟢
### TASK-3.4 — Domain: Repository interfaces removal 🟢
**Note:** Removed per architectural decision to use `IApplicationDbContext` directly.
### TASK-3.5 — Domain: `DomainErrors` — Roster entries 🟢
### TASK-3.6 — Application: Roster commands 🟢
### TASK-3.7 — Application: Roster queries 🟢
### TASK-3.8 — Infrastructure: EF Core configurations 🟢
### TASK-3.9 — API: `RosterController` 🟢
### TASK-3.10 — Tests: Roster unit tests 🟢

---

## Security & Bug Fixes (New)

### TASK-SEC-1 — Secure UsersController 🟢
**What:** Add `[Authorize]` to `UsersController` endpoints. `DELETE` should require admin or self authorization.

### TASK-SEC-2 — Secure Tree Access Endpoints 🟢
**What:** Add `[Authorize(Policy = "TreeOwner")]` to tree access management endpoints in `TreesController`.

### TASK-BUG-1 — Fix Typo in FamilyMember 🟢
**What:** Rename `TransitionToVisbility` to `TransitionToVisibility` in `FamilyMember.cs`.

---

## Documentation & Code Quality (New)

### TASK-DOC-1 — Update design.md 🟢
**What:** Reflect actual architecture (ErrorType, 2-behavior pipeline, ApiControllerBase, DbContext). DONE.

### TASK-DOC-2 — Update Implementation Plan 🟢
**What:** Updated Phase 5 to reflect cookie-based auth (not JWT). Removed FluentValidation references, ValidationPipelineBehavior, and stale gap-filling bullets.

### TASK-DOC-3 — Document Missing Controllers 🟢
**What:** Documented `UsersController` (CRUD + self-only delete) and `TreesController` (with RBAC policies) in the implementation plan.

### TASK-CQ-1 — Relocate RelationshipType 🟢
**What:** Moved `RelationshipType.cs` from `Roster/Entities/` to `Roster/Enums/`. Updated namespace and all consuming usings.

### TASK-CQ-2 — Normalize Table Naming 🟢
**What:** Renamed `family_members` → `roster_family_members` and `family_member_relationships` → `roster_family_member_relationships`. Added EF migration `NormalizeRosterTableNames`.

### TASK-CQ-3 — Remove Dead Repositories 🟢
**What:** Remove dead repository interfaces/implementations. (Marked DONE).

### TASK-CQ-4 — Update Gender Enum 🟢
**What:** Removed legacy `Other` value; enum now has `Male=0, Female=1, NonBinary=2, PreferNotToSay=3`.

---

## Missing Tests (New)

### TASK-TEST-1 — FamilyMemberRelationship Tests 🟢
**What:** Add unit tests for `FamilyMemberRelationship` entity creation and same-member constraints.

### TASK-TEST-2 — AddRelationshipCommandHandler Tests 🟢
**What:** Add unit tests for same-tree constraints and duplicate checking in `AddRelationshipCommandHandler`.

### TASK-TEST-3 — Missing Roster Handler Tests 🟢
**What:** Add unit tests for `RemoveRelationship`, `RequestVisibility`, `UpdateFamilyMember`, `DeleteFamilyMember`.

---

## Future Phases

### Phase 4 — Canvas
🔴 Three-Layer Canvas Model implementation
🔴 Bulk Coordinate Updates implementation

### Phase 5 — Authentication Hardening
🔴 Enforce `[Authorize]` on all development endpoints

### Phase 6 — API Hardening
🔴 Apply rate limiting
🔴 Replace `MemoryDistributedCache` with Redis

### Phase 7 & 8 — Advanced Features
🔴 Async Processing & Vector Search (pgvector)
🔴 Integration Tests with Testcontainers
