# FamilyTreeApp — Frontend Phased Implementation Plan

> **Current state (last reviewed: Initial Design Phase):**
> Frontend application for FamilyTreeApp interacting with the .NET backend API.

## Resolved Decisions

| Concern | Decision | Status |
|---|---|---|
| **Location** | `FamilyTreeApp/FamilyTreeApp.Frontend` | ✅ |
| **Framework** | Next.js 15 (App Router, TypeScript) | ✅ |
| **Styling** | Vanilla CSS with CSS Modules, dark-mode only | ✅ |
| **Authentication** | Google OAuth via backend redirection; HttpOnly cookies (no frontend JWT management) | 🔄 |
| **Client state** | Zustand | ✅ |
| **Server state** | TanStack Query v5 | ✅ |
| **Canvas** | React Flow v12 | ✅ |
| **API Proxy** | Next.js `rewrites` to `localhost:8080` (dev) | ✅ |
| **Canvas Data Source** | Backend `CanvasController` (nodes and edges) | ✅ |
| **Adding Members** | Modal-first flow: fill details → place on canvas | 🔄 |

## Core Structure
The application structure is heavily feature-driven, organized within `src/features/`.
- **`auth`**: Handles login redirection and global auth guards.
- **`dashboard`**: Handles the `TreeTable` and tree creation.
- **`tree-workspace`**: Contains the complex React Flow canvas, sidebar, node components, and canvas state sync.

*(See `tasks.md`, `design.md`, and `requirements.md` for full breakdown of requirements, tasks, and detailed architectural implementations.)*
