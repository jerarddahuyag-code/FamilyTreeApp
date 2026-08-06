# FamilyTreeApp — Implementation Tasks

> **Legend:** 🔴 Not started | 🟡 In progress | 🟢 Complete | ⏭ Deferred
> **Last updated:** Phase 4 Canvas tasks defined

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

## Phase 4 — Canvas

> **Goal:** Build the `TreeNode`, `TreeNodeMember`, and `TreeEdge` entities, the `VisibilityMediator` domain service, and all canvas CRUD endpoints. Implement the frontend canvas fetch/cache/edit flow.

### Domain Layer

#### TASK-4.1 — Domain: `TreeNode` entity 🔴
**What:** Create `FamilyTreeApp.Domain/Canvas/Entities/TreeNode.cs`.
- Properties: `Guid Id`, `Guid TreeId`, `NodeType NodeType`, `CanvasCoordinates Coordinates`, `DateTime CreatedAt`, `DateTime UpdatedAt`.
- Navigation: `ICollection<TreeNodeMember> Members`.
- Domain method: `Result UpdateCoordinates(double x, double y)`.

#### TASK-4.2 — Domain: `TreeNodeMember` join entity 🔴
**What:** Create `FamilyTreeApp.Domain/Canvas/Entities/TreeNodeMember.cs`.
- Composite PK: `(TreeNodeId, FamilyMemberId)`.
- No uniqueness constraint on `FamilyMemberId` alone — a member may appear on multiple nodes.

#### TASK-4.3 — Domain: `TreeEdge` entity 🔴
**What:** Create `FamilyTreeApp.Domain/Canvas/Entities/TreeEdge.cs`.
- Properties: `Guid Id`, `Guid TreeId`, `Guid SourceNodeId`, `Guid TargetNodeId`, `DateTime CreatedAt`, `DateTime UpdatedAt`.

#### TASK-4.4 — Domain: `CanvasCoordinates` value object 🟢
**What:** Create `FamilyTreeApp.Domain/Canvas/ValueObjects/CanvasCoordinates.cs`.
- C# `record` with `double X`, `double Y`.

#### TASK-4.5 — Domain: `NodeType` enum 🟢
**What:** Create `FamilyTreeApp.Domain/Canvas/Enums/NodeType.cs`.
- Enum: `Single`, `Partner`, `MultiPerson`.

#### TASK-4.6 — Domain: `DomainErrors` — Canvas entries 🔴
**What:** Add `CanvasErrors` nested class to `DomainErrors.cs`.
- `MemberNotInTree` — member's TreeId does not match the target canvas node's TreeId.
- `NodeNotInTree` — source or target node's TreeId does not match.
- `NodeNotFound`, `EdgeNotFound`.

#### TASK-4.7 — Domain: `VisibilityMediator` domain service 🔴
**What:** Create `FamilyTreeApp.Domain/Canvas/Services/VisibilityMediator.cs`.
- Input: `IEnumerable<TreeNode>` (with loaded members + family members + users) + requesting user role.
- Logic: For each member — if `VisibilityStatus != Visible` AND requester is not `TreeAdmin` → mask as `{ "firstName": "Anonymous", isMasked: true }`.
- Returns a flat visibility map consumed by `GetCanvasQueryHandler`.

---

### Application Layer

#### TASK-4.8 — Application: `GetCanvasQuery` + handler 🟢
**What:** Create `FamilyTreeApp.Application/Canvas/Queries/GetCanvas/`.
- Query: `Guid TreeId`, `Guid RequestingUserId`.
- Handler: fetches all `TreeNode` entities with `.Include(n => n.Members).ThenInclude(m => m.FamilyMember).ThenInclude(f => f.ClaimedByUser)` + all `TreeEdge` entities for the tree.
- Passes nodes through `VisibilityMediator`.
- Projects into `CanvasDto` (`nodes[]` + `edges[]`).

#### TASK-4.9 — Application: `AddTreeNodeCommand` + handler 🔴
**What:** Create `FamilyTreeApp.Application/Canvas/Commands/AddTreeNode/`.
- Command: `Guid TreeId`, `NodeType NodeType`, `double X`, `double Y`, `IReadOnlyList<Guid> FamilyMemberIds`.
- Validation: all `FamilyMemberIds` must belong to the same `TreeId`. Returns `Result.Failure(DomainErrors.CanvasErrors.MemberNotInTree)` on failure.
- Creates `TreeNode` + `TreeNodeMember` links. Implements `ITransactionalCommand`.

#### TASK-4.10 — Application: `RemoveTreeNodeCommand` + handler 🔴
**What:** Create `FamilyTreeApp.Application/Canvas/Commands/RemoveTreeNode/`.
- Validates node exists and belongs to the tree. Cascade deletes `TreeNodeMember` links.
- Returns `Result.Failure(DomainErrors.CanvasErrors.NodeNotFound)` if not found.

#### TASK-4.11 — Application: `AddTreeEdgeCommand` + handler 🔴
**What:** Create `FamilyTreeApp.Application/Canvas/Commands/AddTreeEdge/`.
- Command: `Guid TreeId`, `Guid SourceNodeId`, `Guid TargetNodeId`.
- Validation: both nodes must exist and belong to the same tree. Returns `Result.Failure(DomainErrors.CanvasErrors.NodeNotInTree)` on mismatch.

#### TASK-4.12 — Application: `RemoveTreeEdgeCommand` + handler 🟢
**What:** Create `FamilyTreeApp.Application/Canvas/Commands/RemoveTreeEdge/`.
- Returns `Result.Failure(DomainErrors.CanvasErrors.EdgeNotFound)` if not found.

#### TASK-4.13 — Application: `UpdateCanvasCommand` + handler 🔴
**What:** Create `FamilyTreeApp.Application/Canvas/Commands/UpdateCanvas/`.
- Command: `Guid TreeId`, `IReadOnlyList<NodePositionUpdate> Updates` where `NodePositionUpdate` is `(Guid NodeId, double X, double Y)`.
- Implements `ITransactionalCommand` — entire batch is atomic.
- Handler loops nodes, calls `UpdateCoordinates()`, and persists.

#### TASK-4.14 — Application: Canvas DTOs 🔴
**What:** Create `FamilyTreeApp.Application/Canvas/DTOs/`.
- `CanvasDto` — `List<TreeNodeDto> Nodes`, `List<TreeEdgeDto> Edges`.
- `TreeNodeDto` — `Guid Id`, `NodeType Type`, `CanvasCoordinates Position`, `List<CanvasMemberDto> Members`.
- `CanvasMemberDto` — `Guid Id`, `ProfileInfo ProfileInfo`, `bool IsMasked`, `VisibilityStatus VisibilityStatus`.
- `TreeEdgeDto` — `Guid Id`, `Guid SourceNodeId`, `Guid TargetNodeId`.

---

### Infrastructure Layer

#### TASK-4.15 — Infrastructure: EF Core configurations 🟢
**What:**
- `TreeNodeConfiguration.cs` — table `canvas_treenode`, `CanvasCoordinates` via `OwnsOne()`, index on `(TreeId)`, FK to `trees_tree` (cascade).
- `TreeNodeMemberConfiguration.cs` — table `canvas_treenode_member`, composite PK `(TreeNodeId, FamilyMemberId)`, FK to `canvas_treenode` (cascade), FK to `roster_family_members` (cascade). **No unique index on `FamilyMemberId` alone.**
- `TreeEdgeConfiguration.cs` — table `canvas_treeedge`, index on `(TreeId)`, FK to `canvas_treenode` for source + target (cascade).

#### TASK-4.16 — Infrastructure: Add Canvas DbSets to `IApplicationDbContext` 🟢
**What:** Add `DbSet<TreeNode>`, `DbSet<TreeEdge>` to `IApplicationDbContext` and `ApplicationDbContext`.
(`TreeNodeMember` accessed via navigation, not direct DbSet.)

#### TASK-4.17 — Infrastructure: EF Core migration `AddCanvas` 🟢
**What:** `dotnet ef migrations add AddCanvas --project FamilyTreeApp.Infrastructure --startup-project FamilyTreeApp.Api`

---

### API Layer

#### TASK-4.18 — API: `CanvasController` 🟢
**What:** Create `FamilyTreeApp.Api/Controllers/CanvasController.cs`.
- `GET /api/v1/trees/{treeId}/canvas` → `[Authorize(Policy = "TreeMember")]` → `GetCanvasQuery`
- `PUT /api/v1/trees/{treeId}/canvas` → `[Authorize(Policy = "TreeAdmin")]` → `UpdateCanvasCommand`
- `POST /api/v1/trees/{treeId}/canvas/nodes` → `[Authorize(Policy = "TreeAdmin")]` → `AddTreeNodeCommand`
- `DELETE /api/v1/trees/{treeId}/canvas/nodes/{nodeId}` → `[Authorize(Policy = "TreeAdmin")]` → `RemoveTreeNodeCommand`
- `POST /api/v1/trees/{treeId}/canvas/edges` → `[Authorize(Policy = "TreeAdmin")]` → `AddTreeEdgeCommand`
- `DELETE /api/v1/trees/{treeId}/canvas/edges/{edgeId}` → `[Authorize(Policy = "TreeAdmin")]` → `RemoveTreeEdgeCommand`

---

### Tests

#### TASK-4.19 — Tests: `VisibilityMediatorTests` 🔴
**What:** Unit tests for `VisibilityMediator`.
- `MaskMember_WhenVisibilityHiddenAndRequesterIsNotAdmin_ReturnsMasked()`
- `MaskMember_WhenVisibilityVisibleAndRequesterIsPublic_ReturnsUnmasked()`
- `MaskMember_WhenUserIsAdminAndMemberIsHidden_ReturnsUnmasked()`

#### TASK-4.20 — Tests: `AddTreeNodeCommandHandlerTests` 🔴
**What:** Unit tests for `AddTreeNodeCommandHandler`.
- `Handle_WhenFamilyMemberBelongsToDifferentTree_ReturnsFailure()`
- `Handle_WhenValidMembersProvided_CreatesNodeAndLinks()`
- `Handle_WhenSameMemberAppearsInMultipleNodes_Succeeds()` (duplication allowed)

#### TASK-4.21 — Tests: `UpdateCanvasCommandHandlerTests` 🔴
**What:** Unit tests for bulk coordinate update.
- `Handle_WhenNodeNotFound_ReturnsFailure()`
- `Handle_WhenAllNodesFound_UpdatesCoordinatesAtomically()`

#### TASK-4.22 — Tests: `GetCanvasQueryHandlerTests` 🟢
**What:** Unit tests for canvas fetch + visibility masking integration.
- Verify masked nodes return anonymized member data.
- Verify admin requesters see unmasked data.

---

### Test Gate

```bash
dotnet ef migrations add AddCanvas --project FamilyTreeApp.Infrastructure --startup-project FamilyTreeApp.Api
dotnet ef database update --project FamilyTreeApp.Infrastructure --startup-project FamilyTreeApp.Api
dotnet build --warnaserror
dotnet test
```

---

## Phase 5 — Authentication Hardening
🔴 Enforce `[Authorize]` on all development endpoints

### Phase 6 — API Hardening
🔴 Apply rate limiting
🔴 Replace `MemoryDistributedCache` with Redis
🔴 Implement `INotificationPublisher` concrete implementation
🔴 Add server-side Redis canvas cache (partitioned by `TreeRole`)

### Phase 7 & 8 — Advanced Features
🔴 Async Processing & Vector Search (pgvector)
🔴 Integration Tests with Testcontainers
