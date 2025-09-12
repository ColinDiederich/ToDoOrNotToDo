# ToDoOrNotToDo — README

## Overview
A small full-stack **to-do app** showcasing clean API design in **.NET 8**, minimal **Vue 3** frontend and **Sqlite** database.

- Backend: .NET 8, MVC Controllers + Services, EF Core + SQLite, Swagger
- Frontend: Vue 3 (Composition API + `<script setup>`), Vite (dev only), Tailwind, Vue Router (history), `fetch`
- Persistence: SQLite file auto-created; seeded on startup with deterministic sample tasks
- UX: single list, add/edit/toggle/delete, sort and search, delete confirmation modal, sound and visual effects, global error banner, pessimistic updates, initial spinner

## Prerequisites
- **.NET 8 SDK** — https://dotnet.microsoft.com/download
- **Node.js (LTS)** — https://nodejs.org/

## Getting Started
Open two terminals (backend and frontend).

### 1) Backend (API on http://localhost:5280)
```bash
cd src/backend/ToDoOrNotToDo.Api
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
- App runs at `http://localhost:5173`.

### 3) Warning: This app has sound
- Disable via toggle in bottom right corner
- Ensure sound levels / headphones are appropriately set to prevent abrupt noise entering your environment

## Backend API Quick Reference
**Base:** `http://localhost:5280/api`

- **GET `/tasks`** → `200 OK` → flat `TaskDto[]` sorted: Active (createdAt asc, id asc), then Completed (completedAt desc, id desc).
- **POST `/tasks`** → `201 Created` + created `TaskDto`.
- **PATCH `/tasks`** → `200 OK` + updated `TaskDto` (id required; at least one of: `title`, `isCompleted`).
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

## UI Behavior
- **Add:** Plus button at bottom of active list → input appears. **Enter**/**Blur** saves; **Esc** cancels.
- **Edit:** Click title (active tasks only). **Enter**/**Blur** save; **Esc** cancels.
- **Complete:** Checkbox left; un-completing clears `completedAt` and returns to active.
- **Delete:** Trash icon → modal. **Enter** or **Delete button click** = confirm; **Esc**, **Cancel button click** or **backdrop click** = cancel.
- **Completed tasks:** Strikethrough; listed below active; **not editable**.
- **Sorting:** Configurable with sorting options at top of task list. Active tasks always above Add button, Completed tasks always below. Sort defaults to Active (oldest first) then Completed (newest first); tie-breakers: active `id asc`, completed `id desc`.
- **Effects Toggle:** Bottom right corner, disables sounds and animations if fun is not allowed.
- **Loading:** Tiny spinner on initial load.
- **Errors:** Global **red** banner (white text). **Auto-dismiss in 5s**.
  - Client-side validation shows **specific** message (title must be non-empty and ≤100).  
  - Server/API failures show **generic** message (“An error occurred, please refresh the browser”).

## Assumptions
- The task list is **small**; returning all tasks in one call is acceptable.
- This is a **single-user** local demo; **no authentication** is included.
- **Pessimistic** request handling (disable controls while requests are in flight) keeps UX predictable.
- All server times are **UTC**; the UI hides timestamps to keep the interface uncluttered.
- The API contract is **decoupled** from persistence via DTOs; EF specifics are not leaked.

## Trade-offs
- **Int IDs** now for readability; **GUIDs** noted for future offline/merge safety & data security.
- **No repository layer** to keep scope small; future: Repository + Specification.
- **Manual mapping** instead of AutoMapper (explicit over abstract for a small app).
- **EnsureCreated** over migrations (fast start; future migrations if schema evolves).
- **fetch** instead of Axios on frontend (no deps; future: interceptors/retries).
- **No ESLint/Prettier** now (minimize setup); future formatting/linting for team hygiene.
- **Swagger always on** for easy review.

## Future Improvements
- **IDs & Concurrency:** Move to **GUIDs** to support offline-first creation and avoid collisions. Add **ETags/If-Match** for optimistic concurrency on updates.
- **Frontend State:** Introduce **Vuex** or **Pinia** for shared state and modular stores as features grow.
- **More Task Properties:** Priorities, tags, due dates. Improve sorting for these new properties.
- **Comprehensive Edit View:** To accomodate new properties, modify edit functionality to expand Task row into edit panel, allowing full configuration of additional properties.
- **Compound Actions:** Through caterpillar button or settings cog. Check off all active tasks, uncheck all completed, clear all completed, empty list, manual database re-sync.
- **HTTP Client:** Switch to **Axios** for interceptors, timeouts, and retry strategies.
- **Quality Gates:** Add **ESLint + Prettier** for consistent formatting; enable **Playwright/Cypress** smoke tests.
- **Logging/Observability:** Adopt **Serilog** (structured JSON logs) or similar utility for request correlation/telemetry/log productionalization.
- **UX Enhancements:** Inline delete confirmations, **drag-and-drop** reordering, per-item loaders, optional timestamp displays, configurable color themes (dark mode ofc).
- **Settings Configurability:** Introduce cog element in same row as header, provide settings menu (inline or modal) to configure all options. Preferences saved to new DB table.
- **Accessibility:** Add ARIA roles/labels, roving focus, and keyboard navigation patterns.
- **Data & Scale:** **Pagination** or infinite scroll for large lists; consider **soft delete** for audit, audit DB tables.
- **Time Types:** Consider utilizing `DateTimeOffset` to preserve caller offsets if local times enter the system.
- **Architecture:** Add **Repository + Specification**; consider **AutoMapper** as DTOs grow.
- **Security & Multi-user:** Add auth (JWT/cookies/login/app integration), user-specific lists, RBAC if multi-tenant.
- **Deployment:** Dockerize backend & frontend; optionally serve frontend from backend in production; move DB to a managed service.
- **CI/CD:** **GitHub Actions** for build/test, lint, and deploy workflows.


### Monitoring & Observability

#### Current State
- **Basic logging**: Console logging for development
- **Health endpoint**: Simple status check

#### Production Requirements
- **Structured logging**: Serilog with JSON formatting
- **Metrics**: Application performance monitoring (APM)
- **Distributed tracing**: Request correlation across services
- **Alerting**: Error rate and response time monitoring
- **Dashboard**: Real-time application health visualization

### Security Considerations

#### Current Implementation
- **No authentication**: Single-user demo application
- **Input validation**: Server-side validation with DataAnnotations
- **CORS configuration**: Restricted to localhost:5173 for development

#### Production Security Requirements
- **Authentication**: JWT or OAuth2 integration
- **Authorization**: Role-based access control for multi-user scenarios
- **Input sanitization**: XSS prevention and SQL injection protection
- **HTTPS enforcement**: TLS termination and secure headers
- **Rate limiting**: API throttling to prevent abuse

### Performance & Scalability

#### Current Limitations
- **Single-user design**: No authentication or user isolation
- **In-memory operations**: All tasks loaded at once (suitable for <1000 tasks)
- **No caching**: Direct database access for all operations

#### Scaling Strategies
- **Pagination**: Implement cursor-based pagination for large datasets
- **Caching**: Add Redis for frequently accessed data
- **CDN**: Static asset delivery for production frontend

### Testing Strategy

#### Current State
- **Manual testing**: cURL examples and manual UI verification
- **No automated tests**: Omitted for brevity in takehome scope

#### Recommended Testing Approach
- **Unit tests**: Service layer business logic and validation
- **Integration tests**: API endpoint testing with test database
- **E2E tests**: Playwright/Cypress for critical user flows
- **Performance tests**: Load testing for API endpoints
- **Contract tests**: API schema validation and backward compatibility

## Monetization Opportunities
- Free with Ads
- Premium Subscription with additional features
  - Export in various formats (pdf/doc/csv/json)
  - AI assistance for to-do list creation and manipulations
  - Multiple lists per user, collaborative lists
