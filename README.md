# ToDoOrNotToDo — README

## Overview
A small full-stack **to-do app** showcasing clean API design in **.NET 8** and a minimal **Vue 3** frontend. The focus is senior-level structure with deliberately small scope and clear future growth paths.

- Backend: .NET 8, MVC Controllers + Services, EF Core + SQLite, Swagger (always on)
- Frontend: Vue 3 (Composition API + `<script setup>`), Vite (dev only), Tailwind, Vue Router (history), `fetch`
- Persistence: SQLite file auto-created; seeded on startup with deterministic sample tasks
- UX: single list, add/edit/toggle/delete, delete confirmation modal, global error banner, pessimistic updates, initial spinner

## Prerequisites (quick links)
- **.NET 8 SDK** — https://dotnet.microsoft.com/download
- **Node.js (LTS)** — https://nodejs.org/

## Getting Started
Open two terminals (backend and frontend).

### 1) Backend (API on http://localhost:5280)
```bash
cd src/backend
dotnet restore
dotnet run
```
- Creates `tasks.db` on first run (if missing), applies **seeding** when empty.
- Swagger UI is available at `http://localhost:5280/swagger`.
- Health check: `GET /api/health`.

### 2) Frontend (Vite dev server)
```bash
cd src/frontend
npm install
npm run dev
```
- App runs at `http://localhost:5173` (Vite default).
- **Vite proxy** is configured so the frontend calls `/api/*` (no CORS setup needed).

## API Quick Reference
**Base:** `http://localhost:5280/api` — JSON, camelCase, omit nulls, ISO-8601 UTC.

- **GET `/tasks`** → `200 OK` → flat `TaskDto[]` sorted: Active (createdAt asc, id asc), then Completed (completedAt desc, id desc).
- **POST `/tasks`** → `201 Created` + created `TaskDto`.
- **PATCH `/tasks`** → `200 OK` + updated `TaskDto` (id required; at least one of `title` or `isCompleted`).
- **DELETE `/tasks/{id}`** → `204 No Content` (idempotent: 204 even if `id` missing).  
- **GET `/health`** → `200 OK` → `{ "status": "ok" }`.

**Errors:** structured envelope
```json
{
  "error": {
    "code": "VALIDATION_ERROR | NOT_FOUND | SERVER_ERROR",
    "message": "Human-readable message",
    "details": {}
  }
}
```

## cURL Examples
```bash
# Health
curl -s http://localhost:5280/api/health

# Get tasks
curl -s http://localhost:5280/api/tasks

# Create
curl -s -X POST http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"New task"}'

# Update title
curl -s -X PATCH http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":1,"title":"Renamed"}'

# Complete
curl -s -X PATCH http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":1,"isCompleted":true}'

# Un-complete
curl -s -X PATCH http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":1,"isCompleted":false}'

# Delete
curl -i -X DELETE http://localhost:5280/api/tasks/1
```

## UI Behavior (at a glance)
- **Add:** Plus button at bottom of active list → input appears. **Enter**/**Blur** save; **Esc** cancels.
- **Edit:** Click title (active tasks only). **Enter**/**Blur** save; **Esc** cancels.
- **Complete:** Checkbox left; un-completing clears `completedAt` and returns to active.
- **Delete:** Trash icon → modal (generic message). **Enter** = confirm; **Esc** or **backdrop click** = cancel.
- **Completed tasks:** Strikethrough; listed below active; **not editable**.
- **Sorting:** Active (oldest first) then Completed (newest first); tie-breakers: active `id asc`, completed `id desc`.
- **Loading:** Tiny spinner on initial load.
- **Errors:** Global **red** banner (white text). **Auto-dismiss in 5s**.  
  - Client-side validation shows **specific** message (title must be non-empty and ≤100).  
  - Server/API failures show **generic** message (“An error occurred, please refresh the browser”).

## Assumptions (concise)
- The task list is **small**; returning all tasks in one call is acceptable.
- This is a **single-user** local demo; **no authentication** is included.
- **Pessimistic** request handling (disable controls while requests are in flight) keeps UX predictable.
- All server times are **UTC**; the UI hides timestamps to keep the interface uncluttered.
- The API contract is **decoupled** from persistence via DTOs; EF specifics are not leaked.

## Trade-offs (highlights)
- **Int IDs** now for readability; **GUIDs** noted for future offline/merge safety.
- **No repository layer** to keep scope small; future: Repository + Specification.
- **Manual mapping** instead of AutoMapper (explicit over abstract for a tiny app).
- **EnsureCreated** over migrations (fast start; future migrations if schema evolves).
- **fetch** instead of Axios (no deps; future: interceptors/retries).
- **No ESLint/Prettier** now (minimize setup); future formatting/linting for team hygiene.
- **Swagger always on** for easy review despite being local-only.

## Seeding
On first run (empty DB), the backend seeds five tasks with deterministic timestamps (UTC) relative to startup time:
- Explore UI/UX (now − 5m)
- Review front-end design (now − 4m)
- Review back-end design (now − 3m)
- Prepare job offer (now − 2m)
- Start task management application — **completed** (created now − 1m, completed now)

## Future Improvements (concise narrative)
- **IDs & Concurrency:** Move to **GUIDs** to support offline-first creation and avoid collisions. Add **ETags/If-Match** for optimistic concurrency on updates.  
- **Frontend State:** Introduce **Pinia** for shared state and modular stores as features grow.  
- **HTTP Client:** Switch to **Axios** for interceptors, timeouts, and retry strategies.  
- **Quality Gates:** Add **ESLint + Prettier** for consistent formatting; enable **Playwright/Cypress** smoke tests.  
- **Logging/Observability:** Adopt **Serilog** (structured JSON logs) and basic request correlation.  
- **UX Enhancements:** Inline delete confirmations, **drag-and-drop** reordering, per-item loaders, optional timestamp displays.  
- **Accessibility:** Add ARIA roles/labels, roving focus, and keyboard navigation patterns.  
- **Data & Scale:** **Pagination** or infinite scroll for large lists; consider **soft delete** for audit.  
- **Time Types:** Consider `DateTimeOffset` to preserve caller offsets if local times enter the system.  
- **Architecture:** Add **Repository + Specification**; consider **AutoMapper** as DTOs grow.  
- **Security & Multi-user:** Add auth (JWT/cookies), user-specific lists, and RBAC if multi-tenant.  
- **Deployment:** Dockerize backend & frontend; optionally serve frontend from backend in production; move DB to a managed service.  
- **CI/CD:** **GitHub Actions** for build/test, lint, and deploy workflows.

## License
MIT (or your choice).
