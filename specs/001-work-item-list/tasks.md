# Tasks: Work Item List

**Input**: Design documents from `/specs/001-work-item-list/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Included. Constitution II and the Quality Gates require a test for each acceptance path this feature changes. Write the story's tests first and confirm they fail before implementing that story.

**Organization**: Tasks are grouped by user story so each story can be implemented and tested on its own.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on incomplete tasks)
- **[Story]**: User story label (US1–US8), only on user-story phases
- Every task names the file it changes

## Path Conventions

- Web app: `backend/WorkItem.Api/`, `backend/WorkItem.Api.Tests/`, `frontend/src/`, `frontend/tests/`
- SQL only in `backend/WorkItem.Api/Data/`. Controllers have no SQL.
- UI uses DaisyUI (`btn`, `table`, `checkbox`, `modal`, `alert`) before any custom widget.
- Chinese copy in the tasks below is binding.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the two projects named in plan.md

- [ ] T001 [P] Create `backend/WorkItem.sln` with a .NET 9 Web API project at `backend/WorkItem.Api/WorkItem.Api.csproj` (Dapper, Microsoft.Data.Sqlite) and an xUnit project at `backend/WorkItem.Api.Tests/WorkItem.Api.Tests.csproj` (Microsoft.AspNetCore.Mvc.Testing) that references the API
- [ ] T002 [P] Scaffold the Vite Vue 3 app in `frontend/` with TypeScript strict, Vue Router, Tailwind CSS, DaisyUI, Vitest, and Vue Test Utils; set `"typecheck": "vue-tsc --noEmit"` and `"test": "vitest run"` in `frontend/package.json`; proxy `/api` to the local API in `frontend/vite.config.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema, shared types, caller identity, and the app shell that every story uses

**⚠️ CRITICAL**: No user story work starts until this phase is complete

- [ ] T003 [P] Create `work_item` and `confirmation` with `PRAGMA foreign_keys = ON` and `CREATE TABLE IF NOT EXISTS` exactly as in `specs/001-work-item-list/data-model.md` in `backend/WorkItem.Api/Data/schema.sql`
- [ ] T004 [P] Add the shared records from `specs/001-work-item-list/contracts/openapi.yaml` (Session, Role, Status, WorkItemSummary, WorkItemDetail, WorkItemPage, AdminWorkItem, AdminWorkItemPage, WorkItemWrite, ConfirmRequest, ConfirmResult, RevokeResult, Error, FieldError) in `backend/WorkItem.Api/Models/ApiModels.cs`
- [ ] T005 [P] Add the same field names as TypeScript types in `frontend/src/api/types.ts`
- [ ] T006 [P] Read `X-Display-Name` and `X-Role`, trim the name, and return 401 when the name is missing in `backend/WorkItem.Api/Auth/CallerContext.cs`
- [ ] T007 Open the SQLite file (configuration default `backend/data/workitems.db`), enable foreign keys, and execute `schema.sql` on startup in `backend/WorkItem.Api/Data/SqliteDatabase.cs`
- [ ] T008 Register SQLite, caller context, and controllers in `backend/WorkItem.Api/Program.cs`
- [ ] T009 Add a `WebApplicationFactory` that uses a temporary SQLite file per test class in `backend/WorkItem.Api.Tests/ApiFactory.cs`
- [ ] T010 [P] Mount the Vue app and declare routes `/sign-in`, `/work-items`, `/work-items/:id`, `/admin/work-items`, `/admin/work-items/new`, and `/admin/work-items/:id/edit` in `frontend/src/main.ts`, `frontend/src/App.vue`, and `frontend/src/router.ts`
- [ ] T011 Add the typed `fetch` helper that sends `X-Display-Name` and `X-Role` on every call in `frontend/src/api/workItems.ts`

**Checkpoint**: API starts, applies the schema, and rejects a call with no display name. The UI shell loads with an empty route table wired.

---

## Phase 3: User Story 1 - Sign in as a front-end user or an administrator (Priority: P1) 🎯 MVP

**Goal**: A person must choose a role and a display name before any list is shown. The trimmed name is the identity.

**Independent Test**: Open the app signed out and see only the sign-in form. Sign in as a front-end user and as an administrator and land in the matching area. Sign in with the same name plus surrounding spaces and get the trimmed name.

### Tests for User Story 1

- [ ] T012 [P] [US1] Add `POST /api/session` tests for a trimmed name, empty-after-trim 400 (`name_required`), and a role other than `user` or `admin` 400 (`invalid_role`) in `backend/WorkItem.Api.Tests/SessionEndpointTests.cs`
- [ ] T013 [P] [US1] Add page tests that the list is hidden until role and name are entered, surrounding spaces are stripped, and each role leaves the sign-in page in `frontend/tests/sign-in.spec.ts`

### Implementation for User Story 1

- [ ] T014 [US1] Implement `POST /api/session` to return the trimmed `{ displayName, role }` in `backend/WorkItem.Api/Controllers/SessionController.cs`
- [ ] T015 [P] [US1] Store `{ displayName, role }` in `localStorage` and restore it on load in `frontend/src/composables/useSession.ts`
- [ ] T016 [US1] Build the sign-in form (Front-end user or Administrator, display name, reject empty after trim) with DaisyUI in `frontend/src/pages/SignInPage.vue`
- [ ] T017 [US1] Send unsigned visitors to `/sign-in`, send `user` to `/work-items`, and send `admin` to `/admin/work-items` in `frontend/src/router.ts`

**Checkpoint**: Sign-in works for both roles. No work item list is visible before a name is stored.

---

## Phase 4: User Story 2 - View the work item list (Priority: P1)

**Goal**: A signed-in front-end user sees 編號, 標題, and 狀態, an empty-state sentence, created-time sort, and pages of 10.

**Independent Test**: Open `/work-items` against an empty database and read 「目前無待辦項目」. Insert items directly in SQLite, reload, and switch sort between newest first and oldest first. With more than 10 items, page 2 shows only that page.

### Tests for User Story 2

- [ ] T018 [P] [US2] Add `GET /api/work-items` tests for an empty `items` array with `total` 0, default `direction=desc`, `direction=asc`, `pageSize` 10, a page past the end, and 401 without a name in `backend/WorkItem.Api.Tests/WorkItemListEndpointTests.cs`
- [ ] T019 [P] [US2] Add page tests for columns 編號, 標題, and 狀態, the text 「目前無待辦項目」, the sort toggle, and a second page in `frontend/tests/work-item-list.spec.ts`

### Implementation for User Story 2

- [ ] T020 [US2] Query `work_item` ordered by `created_at DESC, id DESC` or `created_at ASC, id ASC`, page size 10, `page` 1-based, and status `pending` when that display name has no confirmation row in `backend/WorkItem.Api/Data/WorkItemRepository.cs`
- [ ] T021 [US2] Implement `GET /api/work-items` in `backend/WorkItem.Api/Controllers/WorkItemsController.cs`
- [ ] T022 [P] [US2] Add `listWorkItems(page, direction)` in `frontend/src/api/workItems.ts`
- [ ] T023 [US2] Read and write `page` and `direction` on the `/work-items` query string in `frontend/src/composables/useWorkItemList.ts`
- [ ] T024 [US2] Render the DaisyUI table, 「目前無待辦項目」, the sort control, and the pager; map `pending` to 待確認 and `confirmed` to 已確認 in `frontend/src/pages/WorkItemListPage.vue`

**Checkpoint**: The list is readable and sortable without confirm, detail, or admin screens.

---

## Phase 5: User Story 3 - Confirm and revoke items (Priority: P1)

**Goal**: The signed-in user confirms checked rows on the current page, or revokes one confirmed row, and sees the required toast or dialog. Failures leave the previous status. 最後更新時間 does not change.

**Independent Test**: Check rows, confirm them, cancel a revoke, then revoke one row. A failed save shows an error and keeps the previous status.

### Tests for User Story 3

- [ ] T025 [P] [US3] Add tests for confirm count, already-confirmed ids still counted, empty `ids` 400, missing id 404 with no row written, revoke 200, revoke of a missing id 404, and unchanged `updated_at` in `backend/WorkItem.Api.Tests/ConfirmRevokeEndpointTests.cs`
- [ ] T026 [P] [US3] Add page tests for highlighted checked rows, disabled 確認 until a displayed row is checked, header checkbox select-all and clear of the current page only, mixed header state, toast 「已成功確認 X 項目」, revoke dialog 「確定要將此項目標記回『待確認』嗎？」 with 取消 leaving 已確認 and 確定 showing 「已將 1 項目標記為待確認」, and a failed save keeping the prior status in `frontend/tests/confirm-revoke.spec.ts`

### Implementation for User Story 3

- [ ] T027 [US3] Insert a confirmation row on confirm and delete it on revoke, without updating `work_item.updated_at`, in `backend/WorkItem.Api/Data/WorkItemRepository.cs`
- [ ] T028 [US3] Implement `POST /api/work-items/confirm` (all-or-nothing) and `POST /api/work-items/{id}/revoke` in `backend/WorkItem.Api/Controllers/WorkItemsController.cs`
- [ ] T029 [P] [US3] Add `confirmWorkItems` and `revokeWorkItem` in `frontend/src/api/workItems.ts`
- [ ] T030 [US3] Select and clear only the ids on the current page, including the mixed header checkbox, and clear selection when `page` or `direction` changes in `frontend/src/composables/useWorkItemSelection.ts`
- [ ] T031 [US3] Add row checkboxes, the header checkbox, 確認, 撤銷確認 (only when 已確認), the DaisyUI modal, and success or error alerts in `frontend/src/pages/WorkItemListPage.vue`

**Checkpoint**: Confirm and revoke work on the list for one signed-in name.

---

## Phase 6: User Story 4 - Keep confirmation after returning (Priority: P2)

**Goal**: Refresh or a later sign-in with the same name still shows 已確認, checkboxes start unchecked, and another name still sees 待確認.

**Independent Test**: Confirm an item as A, refresh, sign in again as A, then sign in as B.

### Tests for User Story 4

- [ ] T032 [P] [US4] Add tests that name A stays `confirmed` on a new request and name B stays `pending` for the same item in `backend/WorkItem.Api.Tests/ConfirmationPersistenceTests.cs`
- [ ] T033 [P] [US4] Add page tests that a remounted session still shows 已確認 with every checkbox unchecked, and a second display name shows 待確認, in `frontend/tests/confirmation-persistence.spec.ts`

### Implementation for User Story 4

- [ ] T034 [US4] Restore only `{ displayName, role }` from `localStorage` and never persist checkbox ids in `frontend/src/composables/useSession.ts` and `frontend/src/composables/useWorkItemSelection.ts`

**Checkpoint**: Personal status survives a new page load. Selection does not.

---

## Phase 7: User Story 5 - View work item detail (Priority: P2)

**Goal**: 查看詳情 opens `/work-items/{id}` with the item fields and that user's 狀態. 返回列表 restores the same page and sort with checkboxes unchecked. An unknown id is not found.

**Independent Test**: From page 2 of a newest-first list, open one item and return. Open a missing id.

### Tests for User Story 5

- [ ] T035 [P] [US5] Add `GET /api/work-items/{id}` tests for 編號, title, description, createdAt, caller status, updatedAt, and 404 in `backend/WorkItem.Api.Tests/WorkItemDetailEndpointTests.cs`
- [ ] T036 [P] [US5] Add page tests for those fields, 返回列表 restoring `page` and `direction` with unchecked boxes, and a not-found message with a way back in `frontend/tests/work-item-detail.spec.ts`

### Implementation for User Story 5

- [ ] T037 [US5] Load one item plus the caller status in `backend/WorkItem.Api/Data/WorkItemRepository.cs`
- [ ] T038 [US5] Implement `GET /api/work-items/{id}` in `backend/WorkItem.Api/Controllers/WorkItemsController.cs`
- [ ] T039 [P] [US5] Add `getWorkItem` in `frontend/src/api/workItems.ts`
- [ ] T040 [US5] Show 編號, 標題, 描述, 建立時間, 狀態, and 最後更新時間, plus not-found and 返回列表, in `frontend/src/pages/WorkItemDetailPage.vue`
- [ ] T041 [US5] Add 查看詳情 that keeps the current `page` and `direction`, and do not navigate when the user uses a checkbox, 確認, or 撤銷確認, in `frontend/src/pages/WorkItemListPage.vue`

**Checkpoint**: Detail and return-to-list work from a paged, sorted list.

---

## Phase 8: User Story 6 - Add a work item (Priority: P2)

**Goal**: An administrator creates an item with a required 標題 and optional 描述. A blank title shows a field error and writes nothing. Success returns to the admin list with 「新增成功」. Every front-end user sees the new item as 待確認. A front-end user is refused on admin routes.

**Independent Test**: Submit a blank title, then submit a real title, and find it first on a newest-first list. Open `/admin/work-items` as a front-end user.

### Tests for User Story 6

- [ ] T042 [P] [US6] Add admin list and create tests for 201 with `updatedAt` equal to `createdAt`, newest-first order, blank title 400 with `field: title` and no row, and role `user` 403 in `backend/WorkItem.Api.Tests/AdminCreateEndpointTests.cs`
- [ ] T043 [P] [US6] Add page tests for the field error on an empty or whitespace title, 「新增成功」, the new row first when newest-first, and a front-end user who sees no add, edit, or delete controls in `frontend/tests/admin-create.spec.ts`

### Implementation for User Story 6

- [ ] T044 [US6] Insert a trimmed title, store a missing description as `''`, set both timestamps, and list admin rows without a personal status in `backend/WorkItem.Api/Data/WorkItemRepository.cs`
- [ ] T045 [US6] Implement `GET /api/admin/work-items` and `POST /api/admin/work-items`, returning 403 when the role is not `admin`, in `backend/WorkItem.Api/Controllers/AdminWorkItemsController.cs`
- [ ] T046 [P] [US6] Add `listAdminWorkItems` and `createWorkItem` in `frontend/src/api/workItems.ts`
- [ ] T047 [US6] Render the admin table and the link to `/admin/work-items/new` in `frontend/src/pages/AdminListPage.vue`
- [ ] T048 [US6] Render 標題 (required) and 描述 (optional), block a blank title with a field error, and return to the admin list with alert 「新增成功」 in `frontend/src/pages/AdminWorkItemFormPage.vue`
- [ ] T049 [US6] Refuse role `user` on every `/admin` route so add, edit, and delete controls are not shown in `frontend/src/router.ts`

**Checkpoint**: An administrator can add an item. A front-end user cannot.

---

## Phase 9: User Story 7 - Edit a work item (Priority: P3)

**Goal**: 編輯 opens the form prefilled. A blank title does not save. 儲存 returns with 「更新成功」, changes 最後更新時間, and leaves each person's confirmation as it was.

**Independent Test**: Edit an item that user A has confirmed. A blank title leaves the old title. A valid title is shown, and A still sees 已確認.

### Tests for User Story 7

- [ ] T050 [P] [US7] Add `PUT /api/admin/work-items/{id}` tests for the new title, blank title 400 with the previous row unchanged, confirmation rows unchanged, `updatedAt` changed, and 404 in `backend/WorkItem.Api.Tests/AdminEditEndpointTests.cs`
- [ ] T051 [P] [US7] Add page tests for prefilled 標題 and 描述, a field error that stays on the form, and 「更新成功」 with the new title on the admin row in `frontend/tests/admin-edit.spec.ts`

### Implementation for User Story 7

- [ ] T052 [US7] Update title and description and set `updated_at` to the current UTC time without writing `confirmation` in `backend/WorkItem.Api/Data/WorkItemRepository.cs`
- [ ] T053 [US7] Implement `PUT /api/admin/work-items/{id}` in `backend/WorkItem.Api/Controllers/AdminWorkItemsController.cs`
- [ ] T054 [P] [US7] Add `getAdminWorkItem` and `updateWorkItem` in `frontend/src/api/workItems.ts`
- [ ] T055 [US7] Prefill `/admin/work-items/:id/edit`, keep 儲存 from saving a blank title, and return with alert 「更新成功」 in `frontend/src/pages/AdminWorkItemFormPage.vue`
- [ ] T056 [US7] Add the 編輯 control on each row in `frontend/src/pages/AdminListPage.vue`

**Checkpoint**: Edit updates shared text only. Personal status stays.

---

## Phase 10: User Story 8 - Delete a work item (Priority: P3)

**Goal**: 刪除 asks 「確定要刪除此項目嗎？」. 取消 keeps the row. Success removes it for everyone, shows 「刪除成功」, and leaves other 編號 values unchanged. Failure shows an error and keeps the row. The old detail address is not found.

**Independent Test**: Cancel a delete, then delete the same item, and open the old detail address as a front-end user.

### Tests for User Story 8

- [ ] T057 [P] [US8] Add `DELETE /api/admin/work-items/{id}` tests for 204, cascaded confirmation rows, the remaining item keeping its id, 404, and 403 in `backend/WorkItem.Api.Tests/AdminDeleteEndpointTests.cs`
- [ ] T058 [P] [US8] Add page tests for dialog cancel, 「刪除成功」, a failed delete that keeps the row, and the deleted id showing not-found on the detail route in `frontend/tests/admin-delete.spec.ts`

### Implementation for User Story 8

- [ ] T059 [US8] Delete one `work_item` by id and rely on `ON DELETE CASCADE` for its confirmation rows in `backend/WorkItem.Api/Data/WorkItemRepository.cs`
- [ ] T060 [US8] Implement `DELETE /api/admin/work-items/{id}` in `backend/WorkItem.Api/Controllers/AdminWorkItemsController.cs`
- [ ] T061 [P] [US8] Add `deleteWorkItem` in `frontend/src/api/workItems.ts`
- [ ] T062 [US8] Add the DaisyUI dialog 「確定要刪除此項目嗎？」 with 取消 and 確定, plus alert 「刪除成功」 or an error that keeps the row, in `frontend/src/pages/AdminListPage.vue`

**Checkpoint**: Delete removes the shared item and its personal statuses. Other ids stay.

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Shared failure and display behavior from the spec edge cases and quickstart

- [ ] T063 [P] Format `createdAt` and `updatedAt` as a local date and time in `frontend/src/pages/WorkItemDetailPage.vue` and `frontend/src/pages/AdminListPage.vue`
- [ ] T064 When confirm, revoke, edit, or delete returns 404, show an error and the current list without that item in `frontend/src/pages/WorkItemListPage.vue`, `frontend/src/pages/AdminListPage.vue`, and `frontend/src/pages/AdminWorkItemFormPage.vue`
- [ ] T065 Run the checks in `specs/001-work-item-list/quickstart.md`: `npm run typecheck`, `npm test`, `dotnet test`, and `dotnet build`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup. Blocks every user story
- **User stories (Phase 3–10)**: Depend on Foundational. Later stories can use earlier screens, and each story's tests still run on their own
- **Polish (Phase 11)**: Depends on the stories you intend to ship

### User Story Dependencies

- **US1 (P1)**: After Foundational. No other story
- **US2 (P1)**: After Foundational. Seed rows in SQLite; does not need the admin UI
- **US3 (P1)**: After US2, because confirm and revoke sit on the list page
- **US4 (P2)**: After US1 and US3, because it proves stored status and a restored session
- **US5 (P2)**: After US2, because 查看詳情 is a list control. It does not need US3
- **US6 (P2)**: After US1, because it needs the admin role. The new item's 待確認 status is the US2 list
- **US7 (P3)**: After US6, because it reuses the admin list and form
- **US8 (P3)**: After US6, because delete lives on the admin list. It does not need US7

### Within Each User Story

- Tests are written and fail before that story's implementation
- Repository SQL before the controller
- API client before the composable and page
- The story checkpoint passes before the next story starts

### Parallel Opportunities

- T001 and T002
- T003, T004, T005, T006, and T010
- Each story's backend test and frontend test
- A story's API client task marked [P] beside that story's repository work

---

## Parallel Example: User Story 1

```text
T012 SessionEndpointTests.cs
T013 sign-in.spec.ts
```

After both tests fail:

```text
T014 SessionController.cs
T015 useSession.ts
```

## Parallel Example: User Story 3

```text
T025 ConfirmRevokeEndpointTests.cs
T026 confirm-revoke.spec.ts
```

After both tests fail:

```text
T027 WorkItemRepository.cs confirm and revoke SQL
T029 workItems.ts confirmWorkItems and revokeWorkItem
```

## Parallel Example: User Story 6

```text
T042 AdminCreateEndpointTests.cs
T043 admin-create.spec.ts
```

After both tests fail:

```text
T044 WorkItemRepository.cs insert and admin list
T046 workItems.ts listAdminWorkItems and createWorkItem
```

---

## Implementation Strategy

### MVP First (User Story 1)

1. Finish Phase 1 and Phase 2
2. Finish Phase 3 (sign-in)
3. Stop and run `frontend/tests/sign-in.spec.ts` and `backend/WorkItem.Api.Tests/SessionEndpointTests.cs`

User Story 1 is a complete sign-in slice. The first list a person can use is US1 + US2 + US3, which are all P1.

### Incremental Delivery

1. Setup + Foundational
2. US1 sign-in
3. US2 list (empty state, sort, paging)
4. US3 confirm and revoke
5. US4 status still there after reload
6. US5 detail and 返回列表
7. US6 add
8. US7 edit, then US8 delete
9. Polish, then the quickstart checks

### Parallel Team Strategy

After Phase 2:

- One person takes US1, then US4's session half
- One person takes US2, then US3, then US5
- One person takes US6, then US7 and US8

US7 and US8 can proceed together after US6 only where they touch different files. Both edit `AdminListPage.vue`, so finish US7's list link before US8's delete dialog, or keep delete in its own pass over that file.

---

## Notes

- [P] tasks use different files and do not wait on an unfinished task
- Confirmation is a row in `confirmation` while the person is 已確認. No row means 待確認
- Confirm and revoke must not change `updated_at`. Admin create sets it equal to `created_at`. Admin edit sets it to the current time
- Page size is 10. Sort query values are `desc` and `asc`. Checkbox state is not stored
- Commit after each task or story checkpoint
