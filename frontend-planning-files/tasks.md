# FamilyTree Frontend Implementation Plan

- [ ] **Phase 1: Foundation**
  - [ ] Scaffold Next.js 15 project in `FamilyTreeApp/FamilyTreeApp.Frontend` using `npx create-next-app@latest`.
  - [ ] Configure `next.config.ts` with API proxy to `http://localhost:8080`.
  - [ ] Setup base CSS architecture (`globals.css`, `tokens.css`, `animations.css`).

- [ ] **Phase 2: Core Components & Layouts**
  - [ ] Build shared UI components (Button, Modal, Avatar, Badge, Spinner).
  - [ ] Setup global layouts (Root Layout, App Shell Layout).
  - [ ] Setup API client with `credentials: 'include'` for HttpOnly cookies.
  - [ ] Setup TanStack Query client.

- [ ] **Phase 3: Authentication Feature**
  - [ ] Build `/login` page with "Sign in with Google" button.
  - [ ] Implement auth guard to protect `/dashboard` and `/trees/*` routes.

- [ ] **Phase 4: Dashboard Feature**
  - [ ] Build `TreeTable` component.
  - [ ] Build `CreateTreeModal`.
  - [ ] Integrate with backend tree APIs.

- [ ] **Phase 5: Tree Workspace (Canvas)**
  - [ ] Scaffold `TreeCanvas` using React Flow.
  - [ ] Implement `FamilyMemberNode` custom node.
  - [ ] Implement `FamilyEdge` custom edge.
  - [ ] Build collapsible `WorkspaceSidebar` with tabs.
  - [ ] Implement "Add Member" modal workflow.
  - [ ] Integrate auto-save logic for canvas coordinates.
