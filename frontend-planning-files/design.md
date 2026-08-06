# FamilyTree Frontend Design

## Architecture
The frontend is a Next.js 15 application using the App Router, located in `FamilyTreeApp/FamilyTreeApp.Frontend`.
It serves as the presentation layer for the FamilyTree .NET backend.

## Tech Stack
* **Framework:** Next.js 15 (App Router, TypeScript)
* **Styling:** Vanilla CSS with CSS Modules, utilizing CSS variables for design tokens (Dark mode only).
* **State Management:** 
  * Server State: TanStack Query v5
  * Client State: Zustand
* **Canvas Engine:** React Flow v12
* **Icons:** Lucide React

## Data Flow & Integration
### Authentication
The backend handles Google OAuth and issues HttpOnly cookies. The frontend simply provides a login page that redirects to `/api/Auth/login`. API requests to the backend (`localhost:8080`) will automatically include these cookies (using `credentials: 'include'`).

### Next.js Proxy
During development, Next.js will be configured to proxy API requests to the .NET backend running on `localhost:8080` to avoid CORS issues and simplify cookie management.

### Canvas Data Model
* **Nodes:** Mapped from `TreeNode` and `FamilyMember` data.
* **Edges:** Mapped from `TreeEdge` and `FamilyMemberRelationship` entities provided by the backend.

## Confidence Score
**Confidence Score: 95%**
*Rationale:* The requirements are clearly defined. The transition to a pure Google OAuth flow simplifies the authentication logic significantly. The backend models (`FamilyMemberRelationship` and `TreeEdge`) are confirmed to exist, aligning perfectly with the React Flow requirements.
