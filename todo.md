# TODO Checklist — Task Management App

Use this as your execution checklist. Each item is small, testable, and integrated. Check items off as you complete them.

---

## Preflight
- [ ] Create repo with `/src/backend` and `/src/frontend` folders.
- [ ] Ensure .NET 8 SDK and Node 20+ installed.
- [ ] Decide package managers (npm/pnpm/yarn) and stick to one.

---

## M1 — Scaffolding
- [ ] M1.1 Backend project init (ToDoOrNotToDo.Api).
  - [ ] Add Swagger always on.
  - [ ] Add GET `/api/health` → `{ "status": "ok" }`.
  - [ ] Kestrel on `http://localhost:5280`.
  - [ ] **Verify:** `curl http://localhost:5280/api/health` returns ok.
- [ ] M1.2 JSON options
  - [ ] Configure camelCase + omit nulls.
  - [ ] **Verify:** Sample DTO omits nulls.
- [ ] M1.3 Frontend init
  - [ ] Vite + Vue 3 project.
  - [ ] Tailwind configured with `/src/styles/tailwind.css`.
  - [ ] Base layout and Router scaffolding.
- [ ] M1.4 Vite proxy + Router
  - [ ] Proxy `/api` → `http://localhost:5280`.
  - [ ] **Verify:** Frontend fetch to `/api/health` works.

---

## M2 — Persistence & Domain
- [ ] M2.1 DbContext
  - [ ] Add `AppDbContext` and register with DI.
  - [ ] Connection string: `Data Source=tasks.db`.
  - [ ] EnsureCreated on startup.
  - [ ] **Verify:** `tasks.db` exists after run.
- [ ] M2.2 Task entity
  - [ ] `TaskEntity` with Id, Title (1–100), IsCompleted, CreatedAt, UpdatedAt, CompletedAt? (UTC).
  - [ ] Indexes on CreatedAt, CompletedAt.
  - [ ] **Verify:** Table schema & indexes created.
- [ ] M2.3 Seeding
  - [ ] Seed 5 deterministic tasks when DB empty.
  - [ ] UpdatedAt = CreatedAt on seed.
  - [ ] **Verify:** Query shows expected rows/timestamps.

---

## M3 — API Surface
- [ ] M3.1 DTOs & Mapping
  - [ ] `TaskDto`, `CreateTaskRequest`, `UpdateTaskRequest`.
  - [ ] Mapping helpers Entity↔DTO.
- [ ] M3.2 Domain service
  - [ ] CreateAsync: trim + validate [1–100].
  - [ ] UpdateAsync: title change bumps UpdatedAt; isCompleted flip sets/clears CompletedAt and bumps UpdatedAt; idempotent no-op doesn’t change UpdatedAt.
  - [ ] ListAsync: Active (CreatedAt ASC, Id ASC) then Completed (CompletedAt DESC, Id DESC).
  - [ ] DeleteAsync: idempotent.
- [ ] M3.3 Controllers
  - [ ] GET `/api/tasks` → 200 (sorted).
  - [ ] POST `/api/tasks` → 201.
  - [ ] PATCH `/api/tasks` → 200, idempotent semantics.
  - [ ] DELETE `/api/tasks/{id}` → 204 idempotent.
  - [ ] **Verify (curl):**
    - [ ] GET tasks list.
    - [ ] POST new task.
    - [ ] PATCH title and isCompleted.
    - [ ] DELETE existing/missing id returns 204.
- [ ] M3.4 Error envelope
  - [ ] Global handler/middleware returns:
    ```json
    {
      "error": {
        "code": "VALIDATION_ERROR | NOT_FOUND | SERVER_ERROR",
        "message": "…",
        "details": { }
      }
    }
    ```
  - [ ] **Verify:** invalid POST → 400 with details; PATCH missing id → 404; unhandled → 500.

---

## M4 — Frontend Core UI
- [ ] M4.1 API service
  - [ ] `api.js` with get/create/update/delete and error normalization.
- [ ] M4.2 List component
  - [ ] `TasksList.vue` mounts → spinner → load tasks.
  - [ ] Render Active then Completed with subtle divider.
- [ ] M4.3 Item component
  - [ ] `TaskItem.vue` with checkbox, title, trash; emits `toggle`, `edit`, `delete`.
- [ ] M4.4 Add task flow
  - [ ] “+” button at bottom of Active → input.
  - [ ] Enter/Blur save; Esc cancel; validate [1–100, trimmed].
  - [ ] On save, call create; refresh list; disable controls while awaiting.
- [ ] M4.5 Toggle complete/uncomplete
  - [ ] Wire `toggle` → PATCH isCompleted with pessimistic refresh.
  - [ ] Disable row during request.
- [ ] M4.6 Inline edit (active only)
  - [ ] Click title enters edit mode.
  - [ ] Enter/Blur save; Esc cancel; client validation.
  - [ ] Call update; refresh; disable during request.
- [ ] M4.7 Delete with confirmation
  - [ ] `DeleteModal.vue` (Esc, Enter, backdrop).
  - [ ] Confirm → DELETE; refresh list.

---

## M5 — UX Polish & Hardening
- [ ] M5.1 Global error banner
  - [ ] Red banner; specific client messages; generic server fallback.
  - [ ] Auto-dismiss 5s; simple event bus or lifted state.
- [ ] M5.2 Disable controls during any API call.
- [ ] M5.3 Final wiring
  - [ ] Router root renders `TasksList`.
  - [ ] Proxy verified.
  - [ ] Styles tidy (strikethrough, spacing, divider).
- [ ] M5.4 Smoke tests (curl)
  - [ ] GET /api/health
  - [ ] GET /api/tasks
  - [ ] POST /api/tasks
  - [ ] PATCH /api/tasks
  - [ ] DELETE /api/tasks/{id}

---

## QA / Acceptance Checklist
- [ ] Initial load shows spinner then list.
- [ ] Active above Completed; tie-breakers correct.
- [ ] Add valid → appears at bottom of Active.
- [ ] Add invalid → client message; no API call.
- [ ] Toggle to complete → moves to Completed; UpdatedAt & CompletedAt updated.
- [ ] Toggle to active → returns to Active; CompletedAt cleared.
- [ ] Inline edit active task → saves; invalid blocked with message.
- [ ] Delete → confirmation modal; idempotent success.
- [ ] Error banner visible and auto-dismisses.
- [ ] All requests disable relevant controls while in-flight.

---

## Curl Commands (copy/paste)
```bash
# Health
curl -s http://localhost:5280/api/health

# List (GET)
curl -s http://localhost:5280/api/tasks | jq

# Create (POST)
curl -s -X POST http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"My new task"}' | jq

# Update title (PATCH)
curl -s -X PATCH http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":1,"title":"Updated title"}' | jq

# Toggle complete (PATCH)
curl -s -X PATCH http://localhost:5280/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"id":1,"isCompleted":true}' | jq

# Delete (DELETE, idempotent 204)
curl -i -X DELETE http://localhost:5280/api/tasks/1
```

---

## Definition of Done
- [ ] End-to-end flow implemented and manually verified.
- [ ] No orphan components or endpoints; everything wired from UI → API → DB.
- [ ] README updated with run instructions for backend and frontend.
- [ ] Error handling standardized; validation messages surfaced in UI.
- [ ] Sorting, timestamps (UTC), and idempotency behave per spec.
- [ ] Seeds load deterministically on an empty DB.
