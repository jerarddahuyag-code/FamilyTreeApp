# Rooted Frontend — Phase 1 Implementation Plan

Build the Next.js 15 frontend for the Rooted family tree application. This covers the complete core experience: auth → dashboard → tree workspace with interactive canvas.

## Resolved Design Decisions

| Decision | Choice |
|----------|--------|
| Location | `frontend/` (monorepo sibling to `rooted/`) |
| Framework | Next.js 15, App Router, TypeScript |
| Styling | Vanilla CSS with CSS Modules, dark-mode only |
| Font | Inter (Google Fonts) |
| Icons | Lucide React |
| Client state | Zustand |
| Server state | TanStack Query v5 |
| Canvas | React Flow v12 |
| API client | Custom `fetch` wrapper with JWT refresh logic |
| Forms | React Hook Form + Zod |
| UI components | Fully custom design system (no library) |
| Toasts | Custom built (no library) |
| Animations | CSS-only transitions and keyframes |
| Dev proxy | Next.js `rewrites` → `localhost:8000` |
| CORS | Deferred to a separate backend task |

---

## Proposed Changes

### Phase 1.1 — Project Scaffold & Design System

> Establish the Next.js project, install all dependencies, and build the foundational design system (CSS variables, shared components).

#### [NEW] `frontend/` — Next.js 15 project

Scaffold via `npx create-next-app@latest ./` with TypeScript, App Router, CSS Modules, no Tailwind, no `src/` directory alias (we'll use `src/` explicitly).

```
frontend/
├── public/
├── src/
│   ├── app/                          # Next.js App Router
│   │   ├── layout.tsx                # Root layout (font, providers, global CSS)
│   │   ├── page.tsx                  # Root `/` → redirect logic
│   │   ├── (auth)/                   # Route group: unauthenticated
│   │   │   ├── login/page.tsx
│   │   │   └── register/page.tsx
│   │   ├── (app)/                    # Route group: authenticated
│   │   │   ├── layout.tsx            # App shell (top nav, auth guard)
│   │   │   ├── dashboard/page.tsx
│   │   │   └── trees/[treeId]/page.tsx  # Tree workspace
│   ├── components/
│   │   └── ui/                       # Shared design system
│   │       ├── Button/
│   │       │   ├── Button.tsx
│   │       │   └── Button.module.css
│   │       ├── Input/
│   │       ├── Modal/
│   │       ├── Avatar/
│   │       ├── Badge/
│   │       ├── Dropdown/
│   │       ├── Skeleton/
│   │       ├── Spinner/
│   │       ├── Toast/
│   │       └── Table/
│   ├── features/
│   │   ├── auth/                     # Auth feature
│   │   │   ├── components/           # LoginForm, RegisterForm
│   │   │   ├── hooks/                # useAuth, useAuthGuard
│   │   │   ├── api/                  # login(), register(), refresh(), logout()
│   │   │   └── types.ts              # AuthState, LoginPayload, etc.
│   │   ├── dashboard/                # Dashboard feature
│   │   │   ├── components/           # TreeTable, CreateTreeModal
│   │   │   ├── hooks/                # useTrees
│   │   │   ├── api/                  # fetchTrees(), createTree(), deleteTree()
│   │   │   └── types.ts              # Tree, TreeListResponse
│   │   └── tree-workspace/           # Tree workspace feature
│   │       ├── components/
│   │       │   ├── Canvas/           # React Flow wrapper, custom nodes/edges
│   │       │   ├── Sidebar/          # Collapsible sidebar with tabs
│   │       │   ├── Toolbar/          # Canvas controls bar
│   │       │   └── Roster/           # Member list, forms
│   │       ├── hooks/                # useCanvas, useRoster, useAutoSave
│   │       ├── api/                  # fetchCanvas(), updateCanvas(), fetchMembers(), etc.
│   │       └── types.ts              # TreeNode, FamilyMember, CanvasState
│   ├── lib/
│   │   ├── api-client.ts             # Custom fetch wrapper with JWT interceptor
│   │   ├── query-client.ts           # TanStack Query client config
│   │   └── utils.ts                  # Shared helpers
│   ├── stores/
│   │   ├── auth-store.ts             # Zustand: access token, user profile
│   │   └── ui-store.ts               # Zustand: sidebar state, active tab
│   ├── types/
│   │   └── api.ts                    # Shared API envelope types, error types
│   └── styles/
│       ├── globals.css               # CSS variables, resets, base styles
│       ├── tokens.css                # Design tokens (colors, spacing, radii, shadows)
│       └── animations.css            # Keyframe definitions
├── next.config.ts
├── tsconfig.json
├── package.json
└── .env.local
```

#### [NEW] `frontend/.env.local`

```env
NEXT_PUBLIC_API_URL=http://localhost:3000/api
```

> [!NOTE]
> In dev, the Next.js rewrite proxy forwards `/api/*` to the Django backend at `localhost:8000`. The frontend always hits its own origin. `NEXT_PUBLIC_API_URL` points to the proxied path.

#### [NEW] `frontend/next.config.ts`

```typescript
const nextConfig = {
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'http://localhost:8000/api/:path*',
      },
    ];
  },
};
```

#### [NEW] `frontend/src/styles/tokens.css` — Design tokens

```css
:root {
  /* Colors — Dark mode only, "Rooted" earth palette */
  --color-bg-primary: #0F1419;
  --color-bg-secondary: #1A1F2E;
  --color-bg-surface: #1E2530;
  --color-bg-elevated: #252D3A;
  --color-bg-hover: #2C3644;

  --color-accent-primary: #2D6A4F;      /* Forest green */
  --color-accent-primary-hover: #40916C;
  --color-accent-secondary: #D4A373;     /* Warm amber */
  --color-accent-secondary-hover: #E9C46A;
  --color-accent-danger: #E63946;
  --color-accent-danger-hover: #FF6B6B;

  --color-text-primary: #E8E6E3;
  --color-text-secondary: #9CA3AF;
  --color-text-muted: #6B7280;
  --color-text-inverse: #0F1419;

  --color-border-default: #2C3644;
  --color-border-focus: #40916C;

  /* Edge colors for canvas relationships */
  --color-edge-parent-child: #D4A373;    /* Warm amber */
  --color-edge-spouse: #40916C;          /* Green */

  /* Typography */
  --font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.25rem;
  --font-size-2xl: 1.5rem;
  --font-size-3xl: 2rem;

  /* Spacing */
  --space-1: 0.25rem;
  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-5: 1.25rem;
  --space-6: 1.5rem;
  --space-8: 2rem;
  --space-10: 2.5rem;
  --space-12: 3rem;
  --space-16: 4rem;

  /* Border Radius */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-full: 9999px;

  /* Shadows */
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.3);
  --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.4);
  --shadow-lg: 0 10px 25px rgba(0, 0, 0, 0.5);
  --shadow-glow-green: 0 0 15px rgba(45, 106, 79, 0.3);
  --shadow-glow-amber: 0 0 15px rgba(212, 163, 115, 0.3);

  /* Transitions */
  --transition-fast: 150ms ease;
  --transition-base: 250ms ease;
  --transition-slow: 350ms ease;

  /* Layout */
  --sidebar-width: 320px;
  --sidebar-collapsed-width: 48px;
  --toolbar-height: 48px;
  --topnav-height: 56px;
}
```

#### [NEW] `frontend/src/styles/globals.css`

Global reset, base styles, Inter font import, body theming with `--color-bg-primary` background and `--color-text-primary` text.

#### [NEW] `frontend/src/styles/animations.css`

Reusable keyframe definitions:
- `fadeIn` / `fadeOut` — opacity transitions
- `slideInLeft` / `slideOutLeft` — sidebar expand/collapse
- `slideInUp` — toast enter animation
- `scaleIn` — modal appear
- `pulse` — skeleton loading shimmer
- `spin` — loading spinner

---

### Phase 1.2 — Shared Design System Components

> Build all the custom UI components that will be reused across features.

#### [NEW] `frontend/src/components/ui/Button/`
- Variants: `primary` (green), `secondary` (amber), `ghost` (transparent), `danger` (red)
- Sizes: `sm`, `md`, `lg`
- States: loading (shows spinner), disabled
- Props: `variant`, `size`, `isLoading`, `leftIcon`, `rightIcon`

#### [NEW] `frontend/src/components/ui/Input/`
- Text input with label, error message, helper text
- Variants: default, with left icon, with right action
- States: focus (green border glow), error (red border), disabled

#### [NEW] `frontend/src/components/ui/Modal/`
- Custom dialog component (no Radix)
- Portal-rendered, backdrop click to close, escape key support
- Smooth `scaleIn` CSS animation on open
- Props: `isOpen`, `onClose`, `title`, `children`, `size`
- Focus trap via manual `tabIndex` management

#### [NEW] `frontend/src/components/ui/Avatar/`
- Circular avatar with fallback initials
- Sizes: `xs` (24px), `sm` (32px), `md` (40px), `lg` (56px)
- Props: `src`, `name` (for initials fallback), `size`
- Subtle border ring in `--color-border-default`

#### [NEW] `frontend/src/components/ui/Badge/`
- Small status/role indicator
- Variants: `owner` (amber), `admin` (green), `member` (muted), `public` (blue), `private` (red)
- Props: `variant`, `children`

#### [NEW] `frontend/src/components/ui/Dropdown/`
- Custom dropdown menu (no Radix)
- Click to toggle, outside click to close
- Menu items with optional icons and destructive variant
- Keyboard navigation (arrow keys + enter)

#### [NEW] `frontend/src/components/ui/Skeleton/`
- Loading placeholder with animated pulse/shimmer
- Variants: `text` (line), `circle` (avatar), `rect` (card/row)
- Props: `width`, `height`, `variant`

#### [NEW] `frontend/src/components/ui/Spinner/`
- Small spinning indicator for inline loading states
- CSS `spin` animation
- Sizes: `sm`, `md`

#### [NEW] `frontend/src/components/ui/Toast/`
- Custom toast notification system
- `ToastProvider` wraps the app, renders toast container
- `useToast()` hook returns `toast.success()`, `toast.error()`, `toast.info()`
- Auto-dismiss after 4 seconds, CSS `slideInUp` animation
- Stack from bottom-right, max 3 visible

#### [NEW] `frontend/src/components/ui/Table/`
- Simple table component styled for dark mode
- Header with muted text, rows with hover highlight
- Props: `columns`, `data`, `onRowClick`
- Used by the dashboard tree list

---

### Phase 1.3 — API Client & Auth Infrastructure

> Build the fetch wrapper, Zustand auth store, TanStack Query setup, and auth feature (login + register).

#### [NEW] `frontend/src/lib/api-client.ts`

Custom `fetch` wrapper that:
1. Prepends `NEXT_PUBLIC_API_URL` to relative paths
2. Auto-attaches `Authorization: Bearer <token>` from the Zustand auth store
3. Sets `Content-Type: application/json`
4. Includes `credentials: 'include'` (for HttpOnly refresh cookie)
5. Unwraps the `{ data, meta }` API envelope
6. On `401` response: calls `/auth/token/refresh/`, gets new access token, updates the Zustand store, and retries the original request (once)
7. On refresh failure: clears the auth store, redirects to `/login`
8. Parses RFC 7807 error responses into typed error objects

```typescript
// Conceptual API:
export async function apiClient<T>(url: string, options?: RequestInit): Promise<T>;
```

#### [NEW] `frontend/src/lib/query-client.ts`

TanStack Query client configuration:
- Default `staleTime`: 30 seconds
- Default `retry`: 1
- Global error handler for 401/403
- `QueryClientProvider` wrapper component

#### [NEW] `frontend/src/stores/auth-store.ts`

Zustand store:
```typescript
interface AuthState {
  accessToken: string | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  setAccessToken: (token: string) => void;
  setUser: (user: UserProfile) => void;
  logout: () => void;
}
```

> [!IMPORTANT]
> The access token lives **only in Zustand's in-memory state** — never in localStorage or sessionStorage (per the security spec). On page reload, the token is lost, and the app calls `/auth/token/refresh/` (using the HttpOnly cookie) to silently re-establish the session.

#### [NEW] `frontend/src/stores/ui-store.ts`

Zustand store for UI state:
```typescript
interface UIState {
  sidebarOpen: boolean;
  activeTab: 'roster' | 'settings' | 'details';
  selectedNodeId: string | null;
  toggleSidebar: () => void;
  setActiveTab: (tab: string) => void;
  setSelectedNode: (id: string | null) => void;
}
```

#### [NEW] `frontend/src/types/api.ts`

Shared TypeScript types for the API envelope:
```typescript
interface ApiResponse<T> { data: T; meta: { request_id: string; timestamp: string } }
interface ApiError { type: string; title: string; status: number; detail: string; instance: string }
interface UserProfile { id: string; email: string; global_private_flag: boolean; profile_data: ProfileData; created_at: string; updated_at: string }
interface ProfileData { first_name?: string; last_name?: string; date_of_birth?: string; avatar_url?: string; phone_number?: string; gender?: string; bio?: string }
```

#### [NEW] `frontend/src/features/auth/`

- **`types.ts`** — `LoginPayload`, `RegisterPayload`, `AuthResponse`
- **`api/auth-api.ts`** — `login()`, `register()`, `refreshToken()`, `logout()`, `fetchProfile()`, `updateProfile()`
- **`hooks/useAuth.ts`** — wraps auth API calls with TanStack mutations, updates Zustand store on success
- **`hooks/useAuthGuard.ts`** — hook used in the `(app)` layout to verify auth on mount. Attempts a silent refresh on first load; if it fails, redirects to `/login`
- **`components/LoginForm.tsx`** — React Hook Form with Zod validation (email required, password min 8 chars)
- **`components/RegisterForm.tsx`** — React Hook Form (email, password, confirm password, first name, last name)

#### [NEW] `frontend/src/app/(auth)/login/page.tsx`

Login page:
- Centered card on dark background
- "Rooted" wordmark at top
- `<LoginForm />` component
- Link to register page
- Redirect to `/dashboard` if already authenticated

#### [NEW] `frontend/src/app/(auth)/register/page.tsx`

Registration page:
- Same layout as login
- `<RegisterForm />` component
- Link to login page
- Auto-login after successful registration, redirect to `/dashboard`

#### [NEW] `frontend/src/app/layout.tsx`

Root layout:
- Import Inter font via `next/font/google`
- Import `globals.css`, `tokens.css`, `animations.css`
- Wrap children with `QueryClientProvider` and `ToastProvider`

#### [NEW] `frontend/src/app/page.tsx`

Root page — server component that redirects:
- If user has a valid session → `/dashboard`
- Otherwise → `/login`
- Implementation: attempt `/auth/token/refresh/` server-side; on success redirect, on fail redirect

---

### Phase 1.4 — Dashboard Feature

> Build the authenticated app shell and the dashboard page with tree list.

#### [NEW] `frontend/src/app/(app)/layout.tsx`

Authenticated app shell:
- Runs `useAuthGuard()` — attempts silent refresh, redirects to login if no session
- Top navigation bar: "Rooted" wordmark (left), user avatar + dropdown (right)
- User dropdown: "Profile" (future), "Logout"
- Shows loading skeleton while auth is being verified

#### [NEW] `frontend/src/features/dashboard/types.ts`

```typescript
interface Tree {
  id: string;
  name: string;
  description: string | null;
  is_public: boolean;
  role: 'OWNER' | 'ADMIN' | 'MEMBER';
  created_at: string;
  updated_at: string;
}
```

#### [NEW] `frontend/src/features/dashboard/api/trees-api.ts`

- `fetchTrees()` → `GET /api/v1/trees/`
- `createTree(data)` → `POST /api/v1/trees/`
- `deleteTree(treeId)` → `DELETE /api/v1/trees/{treeId}/`

#### [NEW] `frontend/src/features/dashboard/hooks/useTrees.ts`

TanStack Query hooks:
- `useTrees()` — query for the tree list
- `useCreateTree()` — mutation with cache invalidation
- `useDeleteTree()` — mutation with cache invalidation

#### [NEW] `frontend/src/features/dashboard/components/TreeTable.tsx`

Table component displaying:
- Columns: Tree Name (clickable → navigates to workspace), Role (badge), Public/Private (badge), Created, Actions (dropdown with "Delete" for owners)
- Empty state: illustration + "Create your first family tree" CTA
- Uses `<Table />` and `<Badge />` from the design system
- Row click navigates to `/trees/[treeId]`

#### [NEW] `frontend/src/features/dashboard/components/CreateTreeModal.tsx`

Modal with React Hook Form:
- Fields: Name (required), Description (optional textarea), Public toggle
- Zod validation
- On submit: calls `useCreateTree()`, closes modal, shows success toast

#### [NEW] `frontend/src/app/(app)/dashboard/page.tsx`

Dashboard page:
- Page header: "My Trees" title + "Create Tree" button
- `<TreeTable />` component
- `<CreateTreeModal />` (controlled by local state)
- Loading: table skeleton
- Uses `useTrees()` query

---

### Phase 1.5 — Tree Workspace Feature

> Build the tree workspace: collapsible sidebar, toolbar, and React Flow canvas with custom nodes, edges, and auto-save.

#### [NEW] `frontend/src/features/tree-workspace/types.ts`

```typescript
interface TreeNode {
  id: string;
  tree: string;
  family_member: string;
  canvas_coordinates: { x: number; y: number };
  member: MemberProfile;
  created_at: string;
  updated_at: string;
}
interface MemberProfile {
  id: string;
  visibility_status: 'HIDDEN' | 'PENDING_APPROVAL' | 'VISIBLE';
  profile: ProfileData & { display_name?: string };
  is_masked: boolean;
}
interface FamilyMember {
  id: string;
  tree: string;
  claimed_by_user: string | null;
  visibility_status: string;
  profile: ProfileData;
  created_at: string;
  updated_at: string;
}
```

#### [NEW] `frontend/src/features/tree-workspace/api/`

- `canvas-api.ts` — `fetchCanvas(treeId)`, `updateCanvas(treeId, nodes[])`
- `roster-api.ts` — `fetchMembers(treeId)`, `createMember(treeId, data)`, `updateMember(treeId, memberId, data)`, `deleteMember(treeId, memberId)`
- `tree-detail-api.ts` — `fetchTree(treeId)`, `fetchRBAC(treeId)`

#### [NEW] `frontend/src/features/tree-workspace/hooks/`

- `useCanvas(treeId)` — TanStack query for canvas data
- `useUpdateCanvas(treeId)` — mutation for bulk coordinate update
- `useAutoSave(treeId)` — debounced (2 seconds) auto-save hook. Listens to React Flow's `onNodesChange`, collects dirty positions, triggers `useUpdateCanvas` after debounce
- `useRoster(treeId)` — TanStack query for member list
- `useMemberMutations(treeId)` — create/update/delete member mutations with canvas cache invalidation
- `useTreeDetail(treeId)` — query for tree info + user's role

#### [NEW] `frontend/src/features/tree-workspace/components/Canvas/`

- **`TreeCanvas.tsx`** — React Flow `<ReactFlow>` wrapper. Converts API `TreeNode[]` data into React Flow `Node[]` and `Edge[]` format. Handles:
  - Node drag (admin only, read-only for members)
  - Node selection → updates `selectedNodeId` in UI store → sidebar shows details tab
  - Zoom/pan controls
  - Minimap (toggle from toolbar)
  - Fit-to-view on initial load

- **`FamilyMemberNode.tsx`** — Custom React Flow node component:
  - Rich card design: rounded rectangle with `--color-bg-elevated` background
  - Avatar (top, `<Avatar />` component with fallback initials)
  - Full name (bold, `--color-text-primary`)
  - Relationship label below name (muted text)
  - Bottom bar: claimed badge (✓ green dot) if claimed, visibility icon
  - **Masked variant:** when `is_masked: true`, shows silhouette avatar, "Anonymous Node" text, muted/dimmed styling, no action icons
  - Hover state: subtle scale(1.02) + shadow lift transition
  - Selected state: green border glow (`--shadow-glow-green`)
  - Connection handles: top (target for parent edges) and bottom (source for child edges)

- **`FamilyEdge.tsx`** — Custom React Flow edge component:
  - Straight lines (per design decision)
  - Parent-child edges: `--color-edge-parent-child` (amber)
  - Spouse edges: `--color-edge-spouse` (green)
  - Line thickness: 2px default, 3px on hover
  - Hover: slightly brighter color

#### [NEW] `frontend/src/features/tree-workspace/components/Toolbar/`

- **`CanvasToolbar.tsx`** — Horizontal bar above the canvas:
  - Left: Tree name (editable for admins) + role badge
  - Center: Zoom in/out buttons, fit-to-view, minimap toggle
  - Right: Save status indicator ("Saving..." / "Saved ✓" / "Unsaved changes"), back to dashboard button

#### [NEW] `frontend/src/features/tree-workspace/components/Sidebar/`

- **`WorkspaceSidebar.tsx`** — Collapsible left sidebar:
  - Expand/collapse via toggle button (hamburger icon)
  - When collapsed: shows icon-only tab buttons
  - When expanded: shows full tabbed panels (320px width)
  - CSS transition for smooth expand/collapse (`slideInLeft`)
  - Three tabs (icon + label when expanded):
    1. **Roster** (Users icon) — member list
    2. **Details** (Info icon) — selected node details
    3. **Settings** (Settings icon) — tree config + RBAC

- **`RosterPanel.tsx`** — Roster tab content:
  - Search input to filter members by name
  - Scrollable list of member cards (avatar, name, status badge)
  - "Add Member" button at the top (admin only)
  - Click member → select corresponding canvas node + switch to Details tab

- **`DetailsPanel.tsx`** — Details tab content:
  - Shows when a node is selected (`selectedNodeId` from UI store)
  - Displays full profile data of the selected family member
  - If admin: inline edit form (React Hook Form) for `local_override_data`
  - If member: read-only display
  - If masked/anonymous: shows "This member's information is private"

- **`SettingsPanel.tsx`** — Settings tab content:
  - Tree name + description (editable for owner/admin)
  - Public/private toggle (owner only)
  - RBAC member list: shows all users with roles (read-only for now — role management deferred)

#### [NEW] `frontend/src/features/tree-workspace/components/Roster/`

- **`AddMemberModal.tsx`** — Modal with React Hook Form for creating a new family member:
  - Fields: First name, Last name, Date of birth (optional), Gender (select), Bio (optional)
  - Zod validation
  - On submit: calls `useMemberMutations().create`, invalidates canvas + roster queries, closes modal, shows success toast

- **`EditMemberForm.tsx`** — Inline form in the Details panel for editing member data:
  - Same fields as AddMemberModal but pre-populated
  - Save + Cancel buttons
  - Optimistic update via TanStack Query

#### [NEW] `frontend/src/app/(app)/trees/[treeId]/page.tsx`

Tree workspace page:
- Fetches tree detail + validates user has access (via `useTreeDetail`)
- Renders `<CanvasToolbar />`, `<WorkspaceSidebar />`, `<TreeCanvas />`
- Full viewport height layout (toolbar top, sidebar left, canvas fills rest)
- Loading state: full-page skeleton
- Error state: "Tree not found" or "Access denied" with back-to-dashboard link

---

## User Review Required

> [!IMPORTANT]
> **Edge data model gap.** Your current backend `canvas` API returns `TreeNode` objects with coordinates and member data, but there is **no edge/connection model** in the database. React Flow needs edge data (source node → target node with relationship type) to draw connections. Currently, the API has no way to tell the frontend which nodes are connected.
>
> **Options to resolve:**
> 1. **Add an `Edge` model to the `canvas` app** — a new table storing `source_node_id`, `target_node_id`, `relationship_type` (parent_child, spouse). This is the cleanest approach.
> 2. **Store edges as a JSON field on the `Tree` model** — simpler but less queryable.
> 3. **Derive edges from roster data** — if `FamilyMember` had parent/spouse FK fields, edges could be computed. But your roster model uses JSONB for all data, so there's no structured relationship data.
>
> **We need to decide this before implementation begins.** My recommendation is Option 1 — a proper `Edge` model.

> [!WARNING]
> **Node-to-member relationship type.** The canvas API returns which member a node represents, but doesn't indicate the *relationship between members* (parent, child, spouse). React Flow needs this to draw different edge styles. This is the same issue as above — the edge data must carry the relationship type.

---

## Open Questions

> [!IMPORTANT]
> **Q1: How should family member relationships (parent-child, spouse) be modeled?**
> Currently, `FamilyMember` stores `local_override_data` as unstructured JSONB. There's no structured way to express "Member A is the parent of Member B" or "Member A is married to Member B." The frontend needs this to draw edges on the canvas. Should we:
> - Add a `Relationship` model to the roster app? (e.g., `subject_member`, `related_member`, `relationship_type`)
> - Store relationships as part of the canvas edge data?
> - Use both (roster owns the biological relationship, canvas stores the visual edge)?

> [!IMPORTANT]
> **Q2: What should the "Add Member" workflow look like on the canvas?**
> When an admin clicks "Add Member":
> - Should a new node appear at a default position on the canvas, and the user then fills in details in the sidebar?
> - Or should a modal appear first, the user fills in details, and then the node is placed on the canvas?
> - Should the user be able to drag from an existing node to create a connected member?

---

## Verification Plan

### Automated Tests
We will defer automated frontend tests to a follow-up phase. The initial build will focus on manual verification.

### Manual Verification

1. **Auth flow:**
   - Register a new user → auto-login → lands on dashboard
   - Login with existing credentials → lands on dashboard
   - Refresh the page → silent token refresh → stays logged in
   - Logout → redirected to login, cannot access dashboard

2. **Dashboard:**
   - View list of trees with role badges
   - Create a new tree → appears in list
   - Delete a tree (as owner) → removed from list
   - Click tree → navigate to workspace

3. **Tree workspace:**
   - Canvas loads with nodes positioned at their stored coordinates
   - Nodes display member name, avatar, claimed badge
   - Anonymous nodes show masked placeholder
   - Sidebar toggles open/closed smoothly
   - Roster tab shows member list with search
   - Click a node → sidebar switches to Details tab with member info
   - Admin: drag a node → auto-saves after 2 seconds → "Saved ✓" indicator

4. **Member CRUD (admin):**
   - Add Member → modal appears → fill form → node created on canvas
   - Edit member via Details panel → changes reflected on node
   - Delete member → node removed from canvas

5. **RBAC enforcement:**
   - Member role: cannot drag nodes, cannot see add/edit/delete controls
   - Admin role: can drag, can CRUD members
   - Owner role: can delete tree, can see settings panel
