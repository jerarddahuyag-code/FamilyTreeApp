# FamilyTree Frontend Requirements

## 1. Authentication
* **REQ-AUTH-01:** WHEN an unauthenticated user accesses the application, THE SYSTEM SHALL display a login page with a "Sign in with Google" button.
* **REQ-AUTH-02:** WHEN a user clicks "Sign in with Google", THE SYSTEM SHALL redirect the user to the backend OAuth endpoint (`/api/Auth/login`).
* **REQ-AUTH-03:** WHEN a user successfully authenticates via the backend and is redirected back, THE SYSTEM SHALL maintain their session via HttpOnly cookies.
* **REQ-AUTH-04:** IF a user's session expires or is invalid, THEN THE SYSTEM SHALL redirect them to the login page.

## 2. Dashboard
* **REQ-DASH-01:** WHEN an authenticated user navigates to the dashboard, THE SYSTEM SHALL display a list of family trees they have access to.
* **REQ-DASH-02:** WHEN a user creates a new family tree, THE SYSTEM SHALL prompt for a name, description, and visibility setting.
* **REQ-DASH-03:** WHERE the user is the owner of a tree, THE SYSTEM SHALL allow them to delete the tree.

## 3. Tree Workspace (Canvas)
* **REQ-CANV-01:** WHEN a user opens a tree, THE SYSTEM SHALL load a visual canvas displaying family members as nodes and relationships as edges.
* **REQ-CANV-02:** WHEN a user with admin privileges clicks "Add Member", THE SYSTEM SHALL display a modal to capture the new member's details.
* **REQ-CANV-03:** WHEN the "Add Member" modal is successfully submitted, THE SYSTEM SHALL place the new member node on the canvas.
* **REQ-CANV-04:** WHEN an admin drags a node to a new position, THE SYSTEM SHALL auto-save the canvas state after a brief debounce period.
* **REQ-CANV-05:** WHILE in the tree workspace, THE SYSTEM SHALL display a collapsible sidebar with Roster, Details, and Settings tabs.
* **REQ-CANV-06:** WHEN a node is selected, THE SYSTEM SHALL display the member's profile data in the Details tab.
* **REQ-CANV-07:** IF a member's profile is marked as masked, THEN THE SYSTEM SHALL display a generic "Anonymous Node" on the canvas and hide personal details in the sidebar.
