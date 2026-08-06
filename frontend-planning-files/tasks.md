# FamilyTreeApp Frontend — Implementation Tasks

> **Legend:** 🔴 Not started | 🟡 In progress | 🟢 Complete | ⏭ Deferred

---

## Phase 1 — Foundation

### TASK-1.1 — Scaffold Next.js Project 🟢
**What:** Run `npx create-next-app@latest FamilyTreeApp.Frontend` with App Router, TypeScript, CSS Modules.

### TASK-1.2 — Configure API Proxy 🟢
**What:** Update `next.config.ts` with `rewrites()` forwarding `/api/:path*` to `http://localhost:8080/api/:path*`.

### TASK-1.3 — Base CSS Architecture 🟢
**What:** Create `globals.css`, `tokens.css` (design variables), and `animations.css` in `src/styles/`.

---

## Phase 2 — Core Components & Layouts

### TASK-2.1 — Shared UI Components 🟢
**What:** Implement basic `Button`, `Modal`, `Avatar`, `Badge`, `Spinner`, `Toast`.

### TASK-2.2 — Layout Setup 🟢
**What:** Set up root `layout.tsx` (providers, font) and authenticated app shell layout `(app)/layout.tsx`.

### TASK-2.3 — API Client Setup 🟢
**What:** Create `api-client.ts` implementing a `fetch` wrapper with `credentials: 'include'` for HttpOnly cookies handling.

### TASK-2.4 — TanStack Query & Zustand Setup 🟢
**What:** Configure `query-client.ts`, add `QueryClientProvider` to root layout, and setup Zustand stores (`auth-store.ts`, `ui-store.ts`).

---

## Phase 3 — Authentication Feature

### TASK-3.1 — Login Component 🟢
**What:** Create `/login` page with a "Sign in with Google" button.

### TASK-3.2 — Google OAuth Redirection 🔴
**What:** Button action redirects user to `/api/Auth/login`.

### TASK-3.3 — Auth Store & Auth Guard 🟢
**What:** Implement `useAuthGuard()` hook to check session via `api-client` (e.g. `/api/Users/me`) and redirect unauthenticated users to `/login`.

---

## Phase 4 — Dashboard Feature

> **Goal:** Build the authenticated dashboard displaying family trees.

### TASK-4.1 — API: Trees endpoints 🟢
**What:** Define TanStack queries and mutations in `src/features/dashboard/api/trees-api.ts`.
- `useTrees()` → `GET /api/v1/Trees`
- `useCreateTree()` → `POST /api/v1/Trees`
- `useDeleteTree()` → `DELETE /api/v1/Trees/{id}`

### TASK-4.2 — Component: `TreeTable` 🟢
**What:** Create `src/features/dashboard/components/TreeTable.tsx`.
- Displays list of trees with columns for Name, Role, Public/Private, Actions.
- Clicking a tree row navigates to `/trees/[treeId]`.

### TASK-4.3 — Component: `CreateTreeModal` 🟢
**What:** Create `src/features/dashboard/components/CreateTreeModal.tsx`.
- Form for `Name`, `Description`, `IsPublic`.
- Uses `useCreateTree()` mutation and invalidates `useTrees()` cache on success.

### TASK-4.4 — Page: Dashboard Page 🟢
**What:** Create `src/app/(app)/dashboard/page.tsx`.
- Integrates `TreeTable` and `CreateTreeModal`. Includes loading skeletons and empty states.

---

## Phase 5 — Tree Workspace (Canvas)

> **Goal:** Build the interactive tree workspace with React Flow canvas, syncing with backend Biological (Roster) and Visual (Canvas) layers.

### TASK-5.1 — API: Roster & Canvas endpoints 🟢
**What:** Define queries and mutations in `src/features/tree-workspace/api/`.
- `useCanvas(treeId)` → `GET /api/trees/{treeId}/canvas`
- `useUpdateCanvas()` → `PUT /api/trees/{treeId}/canvas`
- `useAddMember()` → Orchestrates `POST /api/trees/{treeId}/roster` then `POST /api/trees/{treeId}/canvas/nodes`
- `useUpdateMember()` → `PUT /api/trees/{treeId}/roster/{memberId}`

### TASK-5.2 — Component: `TreeCanvas` 🟢
**What:** Create `src/features/tree-workspace/components/Canvas/TreeCanvas.tsx`.
- Wraps `<ReactFlow>` from `@xyflow/react`.
- Maps backend `TreeNodeDto` to React Flow `Node` and `TreeEdgeDto` to React Flow `Edge`.

### TASK-5.3 — Component: `FamilyMemberNode` 🟢
**What:** Create custom React Flow node component for family members.
- Shows Avatar, Name, Relationship/Role.
- Handles `is_masked` boolean from backend (masks name as "Anonymous Node").
- Triggers node selection on click (syncs with Zustand UI store).

### TASK-5.4 — Component: `FamilyEdge` 🟢
**What:** Create custom React Flow edge component.
- Handles styling for parent/child vs spouse relationships based on backend connection types.

### TASK-5.5 — Hook: `useAutoSave` 🟢
**What:** Create `useAutoSave` hook for canvas coordinates.
- Listens to `onNodesChange` from React Flow.
- Debounces coordinate changes and triggers `UpdateCanvasCommand` (`PUT /api/v1/trees/{treeId}/canvas`).

### TASK-5.6 — Component: `WorkspaceSidebar` 🟢
**What:** Create `src/features/tree-workspace/components/Sidebar/WorkspaceSidebar.tsx`.
- Collapsible sidebar with tabs: Roster, Details, Settings.

### TASK-5.7 — Component: `DetailsPanel` 🟢
**What:** Create `src/features/tree-workspace/components/Sidebar/DetailsPanel.tsx`.
- Displays selected node's data. If user is admin, shows editable fields via React Hook Form that triggers `useUpdateMember()`.

### TASK-5.8 — Workflow: "Add Member" Modal 🟢
**What:** Create `src/features/tree-workspace/components/Roster/AddMemberModal.tsx`.
- Form for new member details.
- **Workflow:** 
  1. Submit triggers `useAddMember()`.
  2. Modals closes.
  3. New node appears on the canvas.

### TASK-5.9 — Page: Tree Workspace Page 🟢
**What:** Create `src/app/(app)/trees/[treeId]/page.tsx`.
- Combines `TreeCanvas`, `CanvasToolbar`, and `WorkspaceSidebar`.
- Validates user tree access and fetches initial canvas payload.

