# Prompt Plan — Task Management App (Backend: .NET 8 + EF Core + SQLite; Frontend: Vue 3 + Vite + Tailwind)

This plan is a complete, sequenced blueprint and a set of copy‑pasteable **prompts for a code‑generation LLM**. It prioritizes best practices and incremental progress, ensuring no big jumps in complexity and no orphaned code at any stage.

---

## Blueprint (High-Level)

### Architecture
- **Backend**: .NET 8 Web API (Controllers + Services). EF Core + SQLite (file DB), Swagger always on, deterministic seeding on startup, JSON camelCase + omit nulls, health endpoint.
- **Frontend**: Vue 3 (Composition API + `<script setup>`), Vite dev proxy to `/api`, Tailwind for styling, Vue Router (history mode), native `fetch`, pessimistic updates (disable controls while awaiting), global error banner.
- **Contract**: Single `/api/tasks` resource with GET, POST, PATCH; DELETE `/api/tasks/{id}` (idempotent 204). Error envelope with `code/message/details` for all non-2xx responses.

### Data Model & Semantics
- Entity `Task`: `Id`, `Title`, `IsCompleted`, `CreatedAt`, `UpdatedAt`, `CompletedAt?`.
- **Timestamps**: UTC only; ISO-8601 `Z` over the wire.
- **UpdatedAt changes** when `Title` or `IsCompleted` (and thus `CompletedAt`) change; idempotent PATCH should **not** bump `UpdatedAt`.
- **Sorting**: Active first (CreatedAt ASC, Id ASC tie), then Completed (CompletedAt DESC, Id DESC tie).
- **Validation**: Title trimmed, 1–100 chars, duplicates allowed; PATCH must include at least one of `title` or `isCompleted`.
- **Seeding**: If empty, create 5 tasks with deterministic relative times.

### Frontend UX
- Single list; active above completed (subtle divider).
- Add via “+” at bottom of active list → inline input (Enter/Blur save, Esc cancel).
- Inline edit title for **active** tasks only (Enter/Blur save, Esc cancel).
- Checkbox toggles complete/uncomplete.
- Trash icon opens confirmation modal.
- Initial spinner; global red error banner (auto-dismiss 5s).
- **Pessimistic** updates; disable controls during requests.

---

## Iteration Plan (Milestones → Features)

**M1. Repo & Scaffolding**
1. Create repo structure: `/src/backend`, `/src/frontend`.
2. Backend minimal Web API with health check + Swagger.
3. Frontend Vite + Vue 3 app, Tailwind set up, Router skeleton, dev proxy.

**M2. Persistence & Domain**
4. EF Core DbContext, Task entity, SQLite connection, EnsureCreated.
5. Seed data if empty, deterministic timestamps.

**M3. API Surface**
6. DTOs (TaskDto, CreateTaskRequest, UpdateTaskRequest) + mapping.
7. Service layer (business rules for toggle and UpdatedAt/CompletedAt).
8. Controllers: GET `/api/tasks` (sorted), POST, PATCH (idempotent), DELETE `/api/tasks/{id}` (idempotent 204).
9. Error envelope & validation responses.

**M4. Frontend Core UI**
10. Services/api.js (thin fetch layer with error normalization).
11. Components: `TasksList.vue`, `TaskItem.vue`, `DeleteModal.vue`.
12. Initial load spinner, list rendering with active/completed ordering.
13. Add task (+ button → inline input).
14. Toggle complete/uncomplete with pessimistic updates.
15. Inline edit (active tasks only).
16. Delete with confirmation modal.

**M5. UX Polish & Hardening**
17. Global error banner (specific client validation; generic server error).
18. Disable controls during in-flight requests; auto-dismiss banner.
19. Final wiring: router root → `TasksList`, Vite proxy verified, curl smoke tests.

---

## Second Pass Breakdown (Right-Sizing Each Milestone)

**M1**
- M1.1 Backend project init; add Swagger + health endpoint.
- M1.2 JSON options (camelCase, ignore null).
- M1.3 Frontend Vite init; Tailwind config & base styles.
- M1.4 Vite server proxy `/api` → `http://localhost:5280`; Router stub.

**M2**
- M2.1 Add `AppDbContext`, connection string, EnsureCreated.
- M2.2 Implement `Task` entity + indexes.
- M2.3 Seed function with 5 tasks using `UtcNow` offsets.

**M3**
- M3.1 Define DTOs & validators; manual mapping helpers.
- M3.2 Domain service with rules for UpdatedAt/CompletedAt & idempotency.
- M3.3 Controller endpoints (GET/POST/PATCH/DELETE) + status codes.
- M3.4 Error envelope wrapper & model validation wiring.
- M3.5 Sorting logic check via simple in-memory test (optional).

**M4**
- M4.1 `api.js` with `getTasks`, `createTask`, `updateTask`, `deleteTask`.
- M4.2 `TasksList.vue` renders spinner then list from `getTasks()`.
- M4.3 `TaskItem.vue` shows checkbox/title/trash, emits events.
- M4.4 Add task UX: plus→input; Enter/Esc/Blur behaviors.
- M4.5 Toggle complete/uncomplete (disable controls while awaiting).
- M4.6 Inline edit for active tasks (validation; revert on Esc).
- M4.7 `DeleteModal.vue` + integration in list.

**M5**
- M5.1 Global error banner component + event bus pattern.
- M5.2 Client validation for title; show specific message.
- M5.3 Generic server error path; auto-dismiss after 5s.
- M5.4 Final router wiring → `/` renders `TasksList`; smoke tests: curl + manual.

---

## Third Pass (Micro-Steps You Can Safely Implement)

These are intentionally small but impactful; each step ends integrated.

1) Backend init with Swagger + health.
2) JSON options (camelCase + ignore nulls).
3) Add DbContext + connection string; EnsureCreated; run app.
4) Add `Task` entity with DataAnnotations + indexes.
5) Implement Seed on startup (5 tasks).
6) Create DTOs + mapping functions.
7) Implement domain service (create, update with rules, list sorted, delete).
8) Implement controller endpoints + status codes.
9) Add error envelope middleware/filter; model validation wiring.
10) Frontend init (Vite + Vue + Tailwind); base layout; Router.
11) `api.js` fetch wrappers with error normalization.
12) `TasksList.vue`: spinner + initial GET + grouped rendering.
13) `TaskItem.vue`: presentational with checkbox/title/trash; emits events.
14) Add task flow (+ → input → POST → refresh list).
15) Toggle complete/uncomplete (PATCH) with disabled controls.
16) Inline edit (active only) with validation + revert/cancel.
17) Delete flow with modal (confirm → DELETE → refresh list).
18) Global error banner + auto-dismiss + client validation message path.
19) Final pass: tidy styles, router root to `TasksList`, Vite proxy verify, curl tests.

**Why these are right-sized**: Each step yields a running delta (no orphans), is testable (curl/UI), and advances the spec without big leaps.

---

## Prompts for a Code-Generation LLM

> Each prompt is self-contained, builds on prior steps, and finishes with integration instructions and acceptance checks. Paste them sequentially into your code-gen tool.

### Prompt 1 — Backend scaffold, Swagger, Health
```text
Create a .NET 8 Web API project in /src/backend named ToDoOrNotToDo.Api.

Requirements:
- Enable Swagger in Development AND Production at /swagger.
- Add GET /api/health → 200 {"status":"ok"}.
- Configure Kestrel to listen on http://localhost:5280.

Deliverables:
- Program.cs with Swagger always on.
- Minimal HealthController returning the JSON payload.

Acceptance:
- dotnet run in /src/backend starts on 5280.
- curl http://localhost:5280/api/health returns {"status":"ok"}.
- Swagger UI loads at http://localhost:5280/swagger.
```

### Prompt 2 — JSON options (camelCase, ignore nulls)
```text
Modify the backend to use System.Text.Json with:
- camelCase property naming.
- Ignore null values (WhenWritingNull).

Integrate into Program.cs AddControllers().AddJsonOptions(...).

Acceptance:
- Create a sample DTO in a test endpoint that includes a null field; verify it is omitted in the response.
- Keep /api/health and Swagger working.
```

### Prompt 3 — EF Core + SQLite + DbContext + EnsureCreated
```text
Add EF Core with SQLite.

Tasks:
- Add AppDbContext in /src/backend/Data with DbSet<TaskEntity>.
- Connection string: Data Source=tasks.db (relative to backend folder).
- On startup: EnsureCreated().

Files:
- appsettings.json with "ConnectionStrings:Default": "Data Source=tasks.db".
- Program.cs registers DbContext and ensures database is created at startup.

Acceptance:
- On run, tasks.db file appears in /src/backend.
- No controllers broken.
```

### Prompt 4 — Task entity + indexes
```text
Create TaskEntity in /src/backend/Data/TaskEntity.cs with fields:
- Id (int, PK, identity)
- Title (string, trimmed; 1–100 chars; DataAnnotations)
- IsCompleted (bool)
- CreatedAt (DateTime, UTC)
- UpdatedAt (DateTime, UTC)
- CompletedAt (DateTime?, UTC)

Add EF model configuration:
- Index on CreatedAt and CompletedAt.

Acceptance:
- Build succeeds.
- Database contains Tasks table with columns and indexes.
```

### Prompt 5 — Seed data (deterministic)
```text
Implement seeding on startup if Tasks is empty:
- now = UtcNow
- Insert 5 rows:
  1) Explore UI/UX (createdAt = now−5m)
  2) Review front-end design (now−4m)
  3) Review back-end design (now−3m)
  4) Prepare job offer (now−2m)
  5) Start task management application (completed; createdAt = now−1m; completedAt = now; isCompleted = true)
- Set UpdatedAt = CreatedAt for new rows.

Place in a Seed(AppDbContext) method invoked after EnsureCreated().

Acceptance:
- On fresh DB, GET (temporary endpoint or immediate query) shows 5 tasks inserted with correct timestamps and flags.
```

### Prompt 6 — DTOs + mapping helpers
```text
Create DTOs and mapping in /src/backend/DTOs and /src/backend/Services/Mapping.cs:

DTOs:
- TaskDto: id, title, isCompleted, createdAt, updatedAt, (completedAt optional; omit if null)
- CreateTaskRequest: { title }
- UpdateTaskRequest: { id, title?, isCompleted? } (at least one of title/isCompleted must be present)

Mapping:
- Entity→Dto and CreateRequest→Entity (partial) helpers.

Acceptance:
- Unit build, no runtime yet required.
```

### Prompt 7 — Domain service (business rules)
```text
Add ITasksService and TasksService in /src/backend/Services:

Responsibilities:
- ListAsync(): returns tasks sorted as:
  Active first; within Active: CreatedAt ASC, Id ASC; within Completed: CompletedAt DESC, Id DESC.
- CreateAsync(title): trims; validates 1–100 chars; sets isCompleted=false; sets CreatedAt/UpdatedAt=UtcNow.
- UpdateAsync(id, optionalTitle, optionalIsCompleted):
  - If no fields present → validation error.
  - If isCompleted flips:
      false→true: set CompletedAt=UtcNow; bump UpdatedAt.
      true→false: clear CompletedAt (null); bump UpdatedAt.
  - If title changes (after trim) → bump UpdatedAt.
  - If idempotent (no actual change) → return current entity WITHOUT changing UpdatedAt.
- DeleteAsync(id): delete if exists; if not found, treat as success (idempotent).

Acceptance:
- Methods compile; no controller yet.
```

### Prompt 8 — Controllers (GET/POST/PATCH/DELETE) + status codes
```text
Create TasksController with:
- GET /api/tasks → 200 TaskDto[] (already sorted)
- POST /api/tasks → 201 TaskDto (validates title; duplicates allowed)
- PATCH /api/tasks → 200 TaskDto (requires id; at least one of title/isCompleted present; idempotent behavior as per service)
- DELETE /api/tasks/{id} → 204 No Content (idempotent even if not found)

Also keep GET /api/health.

Acceptance:
- curl GET http://localhost:5280/api/tasks returns list.
- curl POST creates a task.
- curl PATCH updates or idempotently returns unchanged (check UpdatedAt).
- curl -i DELETE returns 204 for both existing and missing ids.
```

### Prompt 9 — Error envelope + validation responses
```text
Implement a consistent error envelope:
{
  "error": {
    "code": "VALIDATION_ERROR | NOT_FOUND | SERVER_ERROR",
    "message": "Human-readable message",
    "details": { ... field-level messages for validation ... }
  }
}

Tasks:
- Add exception filter or middleware that maps:
  - Validation exceptions → 400 with details.
  - Not Found (for PATCH missing id) → 404.
  - Unhandled → 500 with SERVER_ERROR.
- Controllers and service should throw or return results that map to this envelope.

Acceptance:
- Invalid POST title → 400 with details.title message.
- PATCH missing id → 404 with error envelope.
- Random exception path returns 500 with SERVER_ERROR envelope.
```

### Prompt 10 — Frontend scaffold (Vite + Vue + Tailwind + Router + Proxy)
```text
Create /src/frontend as a Vite Vue 3 project.

Requirements:
- Tailwind configured with a small shared layer in /src/styles/tailwind.css.
- Vue Router in history mode; root route renders a placeholder TasksList.
- Vite dev server proxy: '/api' → 'http://localhost:5280'.

Acceptance:
- npm run dev serves http://localhost:5173.
- Navigating to / shows placeholder.
- Fetch to /api/health from the frontend succeeds via proxy.
```

### Prompt 11 — api.js (fetch wrappers + error normalization)
```text
Create /src/frontend/src/services/api.js with native fetch wrappers:

APIs:
- getTasks(): GET /api/tasks
- createTask(title)
- updateTask({ id, title?, isCompleted? })
- deleteTask(id)

Behavior:
- If res.ok: return parsed JSON (or null for 204).
- If not ok: parse error envelope; throw standardized Error with { code, message, details }.

Acceptance:
- Manual test in a temporary setup script shows errors are normalized.
```

### Prompt 12 — TasksList.vue (spinner + grouped rendering)
```text
Implement /src/frontend/src/components/TasksList.vue:

Features:
- On mount: show spinner; call api.getTasks(); then render list.
- Split tasks into Active and Completed arrays per fields in the response; render Active first, then a subtle divider, then Completed (strikethrough styling).
- No editing/controls yet.

Acceptance:
- With seeded backend, UI shows 5 tasks in the described order with a divider.
```

### Prompt 13 — TaskItem.vue (presentational + events)
```text
Create /src/frontend/src/components/TaskItem.vue:

Props:
- task: { id, title, isCompleted }

UI:
- Checkbox on left (checked if isCompleted).
- Title text (strikethrough if completed).
- Trash icon on right.

Emit events:
- 'toggle' when checkbox clicked.
- 'edit' when title clicked (only if not completed).
- 'delete' when trash clicked.

Update TasksList.vue to render TaskItem rows and wire event listeners (no API calls yet).

Acceptance:
- Events log to console when interacting with the row.
```

### Prompt 14 — Add task flow (+ → input)
```text
Enhance TasksList.vue:

Behavior:
- Plus button at the bottom of Active list toggles to a text input.
- Enter = save; Blur = save; Esc = cancel.
- Validate: trim; non-empty; ≤100 chars; if invalid, raise a global error event with a specific message.
- On save: call api.createTask(); on success, refresh list via getTasks(). Disable controls while request is in-flight.

Acceptance:
- Adding valid title inserts a new task at bottom of Active.
- Invalid title shows global error (temporary placeholder banner).
```

### Prompt 15 — Toggle complete/uncomplete (pessimistic)
```text
Wire TaskItem 'toggle' in TasksList.vue:

Behavior:
- When toggled, immediately disable the toggled row’s controls.
- Call api.updateTask({ id, isCompleted: !task.isCompleted }).
- On success, refresh tasks (getTasks()).
- Ensure uncompleting clears CompletedAt in backend (already handled by API).
- Keep pessimistic approach (no local mutation until server returns).

Acceptance:
- Completed tasks move to Completed section; uncompleting moves back to Active, matching sort order.
```

### Prompt 16 — Inline edit (active only)
```text
Implement inline editing in TasksList.vue + TaskItem.vue:

Behavior:
- Clicking the title for an active task enters inline edit mode with a text input (pre-filled).
- Enter or Blur saves; Esc cancels and reverts.
- Client validation (trim; 1–100). On invalid, show global error with specific message; do NOT call API.
- On valid save: api.updateTask({ id, title }), then refresh list.
- Disable the row’s controls while awaiting response.

Acceptance:
- Editing active task title works; completed tasks’ titles are not editable in UI.
```

### Prompt 17 — Delete with confirmation modal
```text
Create DeleteModal.vue and integrate:

DeleteModal.vue:
- Props: visible, message, confirmText, cancelText.
- Keyboard: Esc cancels; Enter confirms; backdrop click cancels.
- Emits confirm/cancel.

In TasksList.vue:
- On TaskItem 'delete', open modal; on confirm: call api.deleteTask(id), then refresh tasks.

Acceptance:
- Deleting an item removes it; deleting a non-existent id (race) still results in refresh (204 idempotent).
```

### Prompt 18 — Global error banner + polish + final wiring
```text
Add a global error banner component (top of app) with:
- Red background, white text.
- Shows specific client validation messages and generic server errors ("An error occurred, please refresh the browser").
- Auto-dismiss after 5 seconds.
- Provide a simple event bus or parent-lifted state to trigger banner messages.

Other:
- Disable relevant controls during any API in-flight call.
- Router root route renders TasksList.
- Confirm Vite proxy works.
- Provide README snippets for curl smoke tests:
  - GET /api/health
  - GET/POST/PATCH/DELETE /api/tasks

Acceptance:
- Full manual walkthrough:
  1) Load → spinner → list.
  2) Add → appears in Active.
  3) Toggle complete/uncomplete → moves between sections with sort rules.
  4) Edit active → saves and reorders where appropriate.
  5) Delete → confirm modal → removed.
  6) Error banner shows and auto-dismisses on simulated errors.
```

### (Optional) Prompt 19 — Sorting verification & idempotent PATCH checks
```text
Add a minimal backend unit/integration test project (optional for now) or a small console harness to verify:
- GET order is Active (CreatedAt ASC, Id ASC), then Completed (CompletedAt DESC, Id DESC).
- PATCH idempotent updates do not change UpdatedAt.

Run tests or console harness and paste the output to confirm.
```

---

## Notes on Best Practices Embedded Above
- **Separation of concerns**: Controllers → Services → DbContext (no separate repo layer for now).
- **Deterministic seeding** and **UTC everywhere** to avoid flaky behaviors.
- **Idempotency** and **standardized error envelope** from day one.
- **Pessimistic updates** to avoid client/server divergence.
- **Progressive enhancement**: every prompt ends integrated (no orphans).
- **Observability**: Swagger always on, curl examples, simple health endpoint.
