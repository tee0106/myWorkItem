# Implementation Plan: Work Item List

**Branch**: `001-work-item-list` | **Date**: 2026-10-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-work-item-list/spec.md`

**Note**: Technical choices are taken from `requirments.md` and constitution 1.0.0. Research in [research.md](./research.md) resolves testing, identity, and persistence. No item remains `NEEDS CLARIFICATION`.

## Summary

A local web app lets a person sign in with a display name as a front-end user or an administrator. Front-end users see a paged, sortable work item list, confirm or revoke items for themselves only, and open a detail page that returns to the same page and sort. Administrators create, edit, and delete the shared items. Confirmation status is stored per display name in SQLite and survives refresh and a later sign-in.

The UI is Vue 3 with TypeScript strict mode, the Composition API, composables, and DaisyUI. The API is a .NET 9 Web API. Data access uses Dapper against a local SQLite file. Entity Framework is not used.

## Technical Context

**Language/Version**: Vue 3 + TypeScript (strict) on the frontend; .NET 9 on the backend

**Primary Dependencies**: Vite, Vue Router, DaisyUI, Tailwind CSS, Dapper, Microsoft.Data.Sqlite

**Storage**: One local SQLite file. Schema is applied with SQL at API startup. `PRAGMA foreign_keys = ON`.

**Testing**: Vitest and Vue Test Utils for page behavior; xUnit with `WebApplicationFactory` against SQLite for the API

**Target Platform**: Local development on Windows. Frontend `npm run dev`. Backend `dotnet run`. No hosted deployment.

**Project Type**: Web application (`frontend/` + `backend/`)

**Performance Goals**: A signed-in user can read a page of 10 items, or the empty-state text, without a noticeable wait on a local machine. Paging covers 100 items (SC-009).

**Constraints**: Routes and Chinese copy in `requirments.md` and `spec.md` are binding. Page size is 10. Sort is created time only. Confirm and revoke do not change 最後更新時間. A display name is the identity; passwords and external sign-in are out of scope. No second ORM, no global state library, no UI kit besides DaisyUI and Tailwind.

**Scale/Scope**: A small local group. Shared work items plus one confirmation status per display name per item. Screens: sign-in, `/work-items`, `/work-items/{id}`, `/admin/work-items`, `/admin/work-items/new`, `/admin/work-items/{id}/edit`.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Checked against constitution **1.0.0** (ratified 2026-10-14).

| Principle | Design | Gate |
|-----------|--------|------|
| I. Code Quality | Vue 3, TypeScript strict, Composition API, composables. .NET 9 Web API. Dapper only. Public API models and composable returns are typed. UI uses DaisyUI (`btn`, `table`, `checkbox`, `modal`, `alert`) before any custom widget. Done means `npm run typecheck` and `dotnet build` both succeed. | PASS |
| II. Testing | Each acceptance path in `requirments.md` gets a test that fails when that path breaks: empty list and sort, per-user confirm and revoke, status after reload, detail fields, admin create/edit/delete including a blank title. Tests assert status text, the disabled 確認 control, toasts, and dialog cancel/confirm. Backend tests use SQLite. Frontend tests drive pages, not private fields. | PASS |
| III. Maintainability | `frontend/` and `backend/` stay separate. SQL lives in the repository class. Controllers have no SQL. Checkbox selection and confirm/revoke calls live in composables. Work item field names are defined once in the API contract and once in the frontend API types, then reused by list, detail, and admin forms. | PASS |
| IV. Complete Changes | 確認 stays disabled until a displayed row is checked. A failed confirm or revoke leaves the previous status. Empty or whitespace titles block create and edit and show a field error. Delete and revoke use the specified dialogs and do nothing on 取消. | PASS |
| V. Simplicity | No Entity Framework, Pinia, Vuex, or second component library. Identity is a trimmed display name plus a session role, stored in `localStorage` and sent on each request. A confirmation row exists only while that person is 已確認. | PASS |

No principle is violated. Complexity Tracking is empty. Implementation must not start while this gate is failing; it is not failing.

### Post-design re-check

Phase 1 adds a SQLite file, two tables, and the REST contract in [contracts/openapi.yaml](./contracts/openapi.yaml). It does not add a project, an ORM, a global store, or an auth framework. Gates I–V still pass.

## Project Structure

### Documentation (this feature)

```text
specs/001-work-item-list/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
└── tasks.md             # created later by /speckit.tasks
```

### Source Code (repository root)

```text
backend/
├── WorkItem.Api/
│   ├── Program.cs
│   ├── Controllers/
│   │   ├── SessionController.cs
│   │   ├── WorkItemsController.cs
│   │   └── AdminWorkItemsController.cs
│   ├── Data/
│   │   ├── schema.sql
│   │   └── WorkItemRepository.cs
│   └── Models/
└── WorkItem.Api.Tests/

frontend/
├── src/
│   ├── main.ts
│   ├── router.ts
│   ├── api/
│   │   └── workItems.ts
│   ├── composables/
│   │   ├── useSession.ts
│   │   ├── useWorkItemList.ts
│   │   └── useWorkItemSelection.ts
│   ├── pages/
│   │   ├── SignInPage.vue
│   │   ├── WorkItemListPage.vue
│   │   ├── WorkItemDetailPage.vue
│   │   ├── AdminListPage.vue
│   │   └── AdminWorkItemFormPage.vue
│   └── components/
└── tests/
```

**Structure Decision**: Option 2, web application. Constitution III requires separate `frontend/` and `backend/` projects. The API project is `backend/WorkItem.Api`. Tests sit beside the project they exercise. Vue pages call composables; composables call `src/api/workItems.ts`. The Vite dev server proxies `/api` to the .NET process so the browser stays on one origin.

## Complexity Tracking

No constitution violations. This table is intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| | | |
