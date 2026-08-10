---
description: 'FamilyTreeApp Backend Architectural Guidelines'
applyTo: '**/*.cs, **/*.csproj'
---

# FamilyTreeApp Backend Context

**1. Technology Stack**
*   **Backend:** ASP.NET Core 10 using strict Clean Architecture (Domain, Application, Infrastructure, Api). Entity Framework Core with PostgreSQL (pgvector), Google OAuth with ASP.NET Core cookie authentication (no custom JWTs), and Serilog.

**2. Architecture & Domain Models**
The system is built heavily on Domain-Driven Design (DDD) and CQRS, with distinct, decoupled modules:
*   **Users & Trees:** Manages users, soft deletes, and `TreeRbac` enforcement (Owner, Admin, Member).
*   **Roster (Biological Layer):** Governed by `FamilyMember` and `FamilyMemberRelationship` aggregates. It tracks biological truth and enforces a `VisibilityStatus` state machine, merging masked anonymous profiles for unauthorized users.
*   **Canvas (Visual Layer):** Governed by `TreeNode`, `TreeNodeMember`, and `TreeEdge`. It maps biological members to visual UI cards (Single, Partner, MultiPerson nodes).

**3. Critical Architectural Rules & Constraints**
*   **Strict Canvas/Roster Segregation:** A non-negotiable domain boundary. Canvas commands (e.g., `AddTreeNodeCommand`) **MUST NOT** mutate Roster entities. 
*   **Member Duplication Allowed:** A `FamilyMember` may appear on multiple `TreeNode` instances within the same canvas (e.g., adoptions). No unique constraint exists on `FamilyMemberId` in the canvas layer.
*   **Data Access:** The Repository pattern is explicitly removed. Use `IApplicationDbContext` directly in command handlers.
*   **Error Handling:** Use the `Result<T>` pattern for domain failures (e.g., validation). Exceptions are reserved for infrastructure faults. The Domain layer must never throw exceptions for business rules.
*   **Dependency Rule:** The `Domain` project has zero external dependencies. The `Api` project only references `Infrastructure` for DI wire-up.
*   **Authentication Claims:** Google claims are explicitly mapped (e.g., `picture`, `given_name`, `family_name`, `openid`, `profile`, `email`). There is no `role` claim from Google. All role-based access to trees is managed via Tree RBAC. There are no high-access endpoints that can CRUD any entity globally.
*   **User Information Propagation:** Commands/Queries that require user information must define specific properties (e.g., `UserId`, `Email`). The controller calling the handlers must parse the required information from the base controller's `User` property and explicitly pass it down.
