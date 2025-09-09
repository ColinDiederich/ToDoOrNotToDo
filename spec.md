# ToDoOrNotToDo — Technical Specification (spec.md)

## 1) Goals & Scope
A compact, senior-quality **to-do task management** app with a .NET backend and Vue frontend that is small in scope, demonstrates good patterns, and leaves room for future improvements.

- **Backend:** .NET 8, MVC Controllers + Services, EF Core + SQLite (file DB), Swagger (always on).
- **Frontend:** Vue 3 (Composition API + `<script setup>`), Vite (dev only), Tailwind (light, tiny shared layer), Vue Router (history mode), native `fetch`.
- **Persistence:** Local SQLite file, created automatically at runtime. **Seeding on startup** if empty, deterministic timestamps.
- **UX:** Minimal UI; single list; add/edit/delete/toggle; delete confirmation modal; global error banner; pessimistic updates; initial spinner.

This spec is the hand-off contract for implementation.

---

## 2) Repository & Local Dev Setup
**Repo name:** `ToDoOrNotToDo`  
**Structure:**
```
/README.md
/src
  /backend
  /frontend
/.gitignore
```
- **Backend** runs at `http://localhost:5280` (fixed port).
- **Frontend** runs at Vite default (e.g., `http://localhost:5173`).
- **Vite dev proxy** forwards `/api/*` to the backend.

We assume **command line** usage (no IDE instructions).

---

## 3) Data Model

### 3.1 Entity: Task
| Field         | Type                | Rules |
|---------------|---------------------|------|
| `Id`          | `int` (PK, auto-inc)| Integer ID, server-assigned. |
| `Title`       | `string`            | Trimmed; 1–100 chars; duplicates allowed. |
| `IsCompleted` | `bool`              | Authoritative completion flag. |
| `CreatedAt`   | `DateTime (UTC)`    | Set on insert. |
| `UpdatedAt`   | `DateTime (UTC)`    | Set on update if any field (**Title**, **IsCompleted**, **CompletedAt**) changes. |
| `CompletedAt` | `DateTime? (UTC)`   | Set/cleared by server when `IsCompleted` true/false. Omitted in JSON when null. |

**Indexes:** `CreatedAt` (for active sort) and `CompletedAt` (for completed sort).

**Storage:** SQLite file `tasks.db` at `/src/backend/tasks.db`. Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=tasks.db"
  }
}
```

**Time:** Store **UTC only** (`DateTime.UtcNow`). API returns ISO-8601 UTC with `Z`.  
**IDs:** Use integer auto-increment. (Future: consider GUIDs for offline/merge safety.)

---

## 4) Sorting & Lists
`GET /api/tasks` returns a **single flat array**, pre-sorted:

1) **Active first**, then **Completed**  
2) Within **Active**: `CreatedAt ASC`, tie-break by `Id ASC`  
3) Within **Completed**: `CompletedAt DESC`, tie-break by `Id DESC`

---

## 5) API Specification

**Base path:** `/api`  
**Content type:** `application/json`  
**JSON conventions:** camelCase; **omit nulls**; timestamps as ISO-8601 UTC (`...Z`).  

### 5.0 Error envelope
```json
{
  "error": {
    "code": "VALIDATION_ERROR | NOT_FOUND | SERVER_ERROR",
    "message": "Human-readable message",
    "details": {
      // For VALIDATION_ERROR, this may contain field-specific errors, e.g.,
      // "title": "Title must be between 1 and 100 characters."
    }
  }
}
```
**Backend note:** Populate `error.details` with field-level messages for `400 Bad Request` validation failures.

### 5.1 DTOs

**TaskDto**
```json
{
  "id": 1,
  "title": "Review back-end design",
  "isCompleted": false,
  "createdAt": "2025-09-08T21:00:00Z",
  "updatedAt": "2025-09-08T21:05:00Z"
  // "completedAt": "2025-09-08T21:06:00Z"  (present only if completed)
}
```

**CreateTaskRequest**
```json
{ "title": "string (trimmed; 1–100 chars; duplicates allowed)" }
```

**UpdateTaskRequest**
```json
{
  "id": 123,
  "title": "optional (trimmed; 1–100 chars)",
  "isCompleted": true
}
```
> At least one of `title` or `isCompleted` **must** be present.

### 5.2 Endpoints & Status Codes

- **GET `/api/tasks`** → `200 OK`  
  Returns: `TaskDto[]` (**single flat array, already sorted per §4**).

- **POST `/api/tasks`** → `201 Created` + **TaskDto**  
  Body: `CreateTaskRequest`. Server sets `id`, `createdAt`, `updatedAt`, `isCompleted=false`, `completedAt=null`.

- **PATCH `/api/tasks`** → `200 OK` + **TaskDto**  
  Body: `UpdateTaskRequest`. Server auto-sets/clears `completedAt`. **If the update results in no change (e.g., setting `isCompleted: true` when it's already true), return `200 OK` with the current `TaskDto` and do not change `updatedAt` (idempotent).**  
  **Missing `id`** → `404 Not Found`.

- **DELETE `/api/tasks/{id}`** → `204 No Content`  
  **If `id` not found, still `204`** (idempotent delete).

- **GET `/api/health`** → `200 OK`  
  Body: `{ "status": "ok" }`

**Validation Errors:** `400 Bad Request` with structured error envelope.  
**Server Errors:** `500 Internal Server Error` with structured error envelope.

### 5.3 Semantics & Rules
- `UpdateTaskRequest`: if `isCompleted` flips from false→true, set `CompletedAt = UtcNow`. If true→false, **clear** `CompletedAt` (null). **`UpdatedAt` mutates if `Title` changes or if `isCompleted` (and thus `completedAt`) changes.**  
- Editing **completed** tasks’ **titles** is **allowed** at the API level (UI doesn’t expose it).  
- JSON omits nulls (e.g., no `completedAt` when active).

---

## 6) Backend Design

- **Project:** Single Web API project with folders: `Controllers/`, `Services/` (business logic), `Data/` (DbContext, entity), `DTOs/`.  
- **DbContext:** EF Core with SQLite. `Database.EnsureCreated()` on startup; no migration files.  
- **Service layer:** Talks to `DbContext` **directly** (no repository); mention Repository + Specification in README as a future improvement.  
- **Mapping:** **Manual** (Entity ↔ DTO).  
- **Validation:** DataAnnotations for field rules, plus **manual check** in PATCH to ensure at least one of `title` or `isCompleted` is present.  
- **Swagger:** Enabled **always** (development + production) at `/swagger`.  
- **Logging:** Default .NET console logging (future: Serilog).  
- **JSON options:** camelCase, **ignore nulls**.  
- **Seeding:** On startup **if DB empty**, insert five tasks with deterministic timestamps (see §8).  
- **Ports:** Backend on **5280**.  
- **Health check:** `/api/health`.

---

## 7) Frontend Design

### 7.1 Tech & Structure
- **Vue 3** (Composition API) with **`<script setup>`** + **Vite** (dev only).  
- **Vue Router** in **history mode** (single route for `TasksList`).  
- **Tailwind CSS**: light usage + **tiny shared layer** (`styles/tailwind.css`) for buttons/modals/banner.  
- **Fetch** for HTTP (future: Axios).  
- **Directory layout:**
```
/src/frontend/src
  /components
    TasksList.vue
    TaskItem.vue
    DeleteModal.vue
  /services
    api.js
  /styles
    tailwind.css
  App.vue
  main.js
  router.js
/vite.config.js
```

### 7.2 Vite Dev Proxy
`vite.config.js`:
```js
export default {
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5280',
        changeOrigin: true
      }
    }
  }
};
```

### 7.3 UI/UX Behavior
- **List layout:** Single vertical list. **Active** tasks first; **Completed** below, separated by a **subtle divider**, no headers.  
- **Add new task:** **Plus button at the bottom** of the active list turns into a text input. **Enter** = save, **Escape** = cancel, **Blur** = save.  
- **Edit title:** Click title to inline edit (only for active tasks). **Enter** = save, **Escape** = cancel, **Blur** = save.  
- **Complete/uncomplete:** Checkbox at left. Uncomplete **clears** `completedAt` and moves task back to active list.  
- **Delete:** Trash icon at right opens **DeleteModal** (generic text).  
  - Modal supports **Esc = cancel**, **Enter = confirm**, and **backdrop click = cancel**.  
- **Completed tasks:** Show with **strikethrough**. **Titles not editable** in UI.  
- **Timestamps:** **Hidden** in UI.  
- **Loading:** **Spinner** shown on initial load only.  
- **Error handling:** **Global red banner** (white text) at top.  
  - **Auto-dismiss after 5s.**  
  - **Client-side validation** shows a **specific** message (“Task title must be non-empty and ≤100 characters”).  
  - **Server/API failures** show a **generic** message (“An error occurred, please refresh the browser”).  
- **Request strategy:** **Pessimistic updates** for all actions (create/edit/toggle/delete). Disable relevant controls while requests are in-flight (no inline spinners necessary).

### 7.4 Client Validation
- Title is trimmed; must be 1–100 chars.  
- If invalid: **block request** and show the global banner with the **specific** message.

---

## 8) Seeding (Deterministic)
On startup, if the DB is empty, insert the following in UTC with deterministic offsets relative to `now = UtcNow`:

- **Explore UI/UX** → `createdAt = now − 5 min`
- **Review front-end design** → `createdAt = now − 4 min`
- **Review back-end design** → `createdAt = now − 3 min`
- **Prepare job offer** → `createdAt = now − 2 min`
- **Start task management application** *(completed)* → `createdAt = now − 1 min`, `completedAt = now`, `isCompleted = true`

---

## 9) Non-Functional
- **Performance:** Return **full list** in one response (assume small sets). (Future: pagination / infinite scroll.)  
- **Security:** **No auth** (single-user demo).  
- **Deploy:** Local dev only (no Docker, no static hosting from backend).  
- **Testing:** Omitted for brevity; covered in README future improvements.  
- **Accessibility:** Minimal; call out improvements in README.

---

## 10) Implementation Notes (Backend)
- Configure JSON to ignore nulls and use camelCase:
  ```csharp
  builder.Services.AddControllers().AddJsonOptions(o =>
  {
      o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
      o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
  });
  ```
- Enable Swagger always:
  ```csharp
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();
  var app = builder.Build();
  app.UseSwagger();
  app.UseSwaggerUI();
  ```
- Ensure DB and seed on startup (pseudo-code):
  ```csharp
  using var scope = app.Services.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.EnsureCreated();
  if (!db.Tasks.Any()) Seed(db);
  ```

---

## 11) Implementation Notes (Frontend)
- **Router:** History mode; root route shows `TasksList`.  
- **API module (`services/api.js`):** thin wrappers over `fetch`, check `res.ok`, parse JSON (or empty on 204), throw standardized errors.  
- **State:** Local component refs; no store (Pinia) for this scope.  
- **Tailwind:** Minimal shared styles in `styles/tailwind.css` for buttons, modal base, banner.

---

## 12) Curl Examples
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

---

## 13) Decisions & Trade-offs (Summary)
- **Pessimistic UI updates** for all actions (simpler, predictable).  
- **DTOs + manual mapping** for a clean API contract.  
- **Service → DbContext directly** (simple now; future: Repository + Specification).  
- **SQLite + EnsureCreated + seeding** (quick start; future: EF migrations).  
- **Swagger always on** for easy review.  
- **Vue 3 + Tailwind + Router + fetch** (lean, modern).  
- **Global red banner** with **auto-dismiss (5s)**; specific client-side validation messages; generic server errors.

---

## 14) Out-of-Scope / Future (high-level)
Details and rationale live in README:  
- GUID IDs, state management (Pinia), Axios, ESLint/Prettier, e2e tests, Docker & static hosting, Serilog, accessibility, pagination, DnD reordering, soft delete, concurrency control (ETags/If-Match), AutoMapper, DateTimeOffset, multi-user & auth, CI/CD (GitHub Actions).
