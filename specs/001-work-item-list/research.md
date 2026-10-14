# Research: Work Item List

Resolved before design. Sources: `requirments.md` Technical Constraints, constitution 1.0.0, and `specs/001-work-item-list/spec.md`. Nothing is left as NEEDS CLARIFICATION.

## 1. Application stack

**Decision**: Vue 3, TypeScript strict, Composition API, composables, Vite, Vue Router, DaisyUI on Tailwind CSS. Backend is .NET 9 Web API with Dapper and Microsoft.Data.Sqlite. Database is one local SQLite file.

**Rationale**: `requirments.md` and constitution I and Technical Constraints name this stack and forbid Entity Framework. DaisyUI supplies button, table, checkbox, modal, and alert, which cover confirm, revoke, delete, and toasts.

**Alternatives considered**: Entity Framework was rejected because the constitution forbids it. A second UI kit or a global store (Pinia) was rejected by principle V. A single full-stack project was rejected by principle III.

## 2. How a person is remembered

**Decision**: Sign-in is a local form at `/sign-in`. The person picks Front-end user or Administrator and enters a display name. The client trims the name, rejects an empty result, and stores `{ displayName, role }` in `localStorage`. Each API call sends `X-Display-Name` and `X-Role`. The server trims again and returns 401 when the name header is missing. Admin routes return 403 when the role is not `admin`. Closing the browser keeps the name, so the next visit restores that person's statuses. Choosing the other role with the same name does not create a second person.

**Rationale**: The spec says a trimmed display name is the identity and that passwords and external sign-in are out of scope. Principle V says authentication stays only as large as per-user status requires. The server stores confirmation by display name, so refresh and a later visit keep 已確認 without a user table.

**Alternatives considered**: ASP.NET Identity and passwords were rejected as larger than the spec. An http-only session cookie was rejected because `localStorage` plus two headers is enough for a local app and is easier for page tests to set. A users table was rejected because the name on the confirmation row is the identity.

## 3. Where personal status lives

**Decision**: Table `work_item` holds the shared item. Table `confirmation` holds one row per display name and work item only while that person is 已確認. No row means 待確認. Confirm inserts the row. Revoke deletes it. Deleting a work item deletes its confirmation rows. Admin edits of title or description do not write `confirmation` and do not change `updated_at` except for the edit itself. Confirm and revoke do not change `updated_at`.

**Rationale**: Status is personal (FR-017) and a new item starts as 待確認 for everyone (FR-018). Storing only the confirmed state avoids a row for every person. 最後更新時間 is defined as the shared item's last admin write (FR-033).

**Alternatives considered**: A status column on `work_item` was rejected because one column cannot differ per person. Storing an explicit pending row for every viewer was rejected as extra writes with the same observable result.

## 4. Identifiers, time, sort, and paging

**Decision**: `work_item.id` is `INTEGER PRIMARY KEY AUTOINCREMENT`. That value is 編號. SQLite does not reuse it after delete. Timestamps are UTC ISO-8601 text. The UI formats them with the browser's local date-and-time. Default order is `created_at DESC, id DESC`. Ascending is `created_at ASC, id ASC`. Page size is fixed at 10. List URL query `page` and `direction` (`desc` or `asc`) are the source of truth, so 返回列表 restores them. Changing page or direction clears checkboxes in the selection composable.

**Rationale**: FR-032 requires a stable number that is not reused. FR-005, FR-006, and FR-021 require created-time sort, 10 per page, and restored page and sort. The second sort key keeps order stable when two items share a timestamp.

**Alternatives considered**: GUIDs were rejected because 編號 is a number people see. Client-only sort of a full download was rejected because paging is part of the contract and select-all applies to the current page only.

## 5. API shape

**Decision**: JSON REST under `/api`, documented in `contracts/openapi.yaml`. Status values on the wire are `pending` and `confirmed`. The Vue pages map those to 待確認 and 已確認 and own the Chinese toasts and dialogs. A confirm or revoke whose id is missing returns 404 and changes nothing. Create and edit reject a blank or whitespace title with HTTP 400 and `field: title`. The Vite dev server proxies `/api` to the API.

**Rationale**: The plan command asks for an endpoint per user action. Keeping Chinese copy in the pages lets API tests assert status codes and counts, and page tests assert the required sentences. Proxying avoids a CORS package.

**Alternatives considered**: GraphQL was rejected because the actions are a short list of reads and writes. Returning Chinese sentences from the API was rejected so the contract stays in the domain words the constitution asks for (`title`, `status`, `confirmation`).

## 6. Tests

**Decision**: Backend tests are xUnit using `WebApplicationFactory` and a temporary SQLite file per test class. Frontend tests are Vitest and Vue Test Utils, mounting the routed pages with a fake API client. Tests cover the acceptance paths named in constitution II: list order and empty state, per-user confirm and revoke, status after a new page load, detail fields, and admin create, edit, and delete, including validation failure, dialog cancel, and a failed save that leaves the prior status.

**Rationale**: Constitution II requires automated tests of observable behavior against SQLite, and forbids frontend tests of private fields. These two runners are the usual ones for Vite and .NET and do not add a second application framework.

**Alternatives considered**: Playwright was rejected for the first test layer because the same assertions can run in Vue Test Utils without a browser. It can be added later only if a page test cannot see a dialog or toast. NUnit was rejected in favor of xUnit, which ships with the ASP.NET test host samples.

## 7. Schema startup

**Decision**: On startup the API opens the SQLite file (path from configuration, default `backend/data/workitems.db`), enables foreign keys, and runs `schema.sql` with `CREATE TABLE IF NOT EXISTS`. No migration library.

**Rationale**: The database is a local file with two tables. A migration tool would violate principle V.

**Alternatives considered**: Entity Framework migrations were rejected with Entity Framework. Hand-written migration versions were rejected until a second released schema exists.
