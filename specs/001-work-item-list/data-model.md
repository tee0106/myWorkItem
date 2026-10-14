# Data Model: Work Item List

Entities from `spec.md`. Persistence details from [research.md](./research.md).

## Person

Not a table. A person is the trimmed display name plus the role chosen for this visit.

| Field | Rules |
|-------|--------|
| displayName | Required after trim. Leading and trailing spaces are removed. The trimmed value is the identity. Two different spellings are two people. |
| role | `user` (Front-end user) or `admin` (Administrator). Chosen at sign-in. Not stored in SQLite. The same name may sign in as either role. |

Personal confirmation is recorded for a display name when that person confirms an item. An administrator session does not need its own confirmation rows; the same name signed in as a front-end user sees the rows stored for that name.

## Work Item

Shared item. Every signed-in person sees the same content.

| Field | Storage | Rules |
|-------|---------|--------|
| id | `work_item.id` INTEGER PRIMARY KEY AUTOINCREMENT | 編號. Assigned on create. Unique. Never reused or renumbered after another item is deleted. |
| title | `title` TEXT NOT NULL | Required. Trimmed. Empty or whitespace-only is rejected on create and edit. Duplicate titles are allowed. |
| description | `description` TEXT NOT NULL DEFAULT `''` | Optional. Blank is stored as empty string and still shown on the detail page. |
| createdAt | `created_at` TEXT NOT NULL | UTC ISO-8601. Set once on create. 建立時間. |
| updatedAt | `updated_at` TEXT NOT NULL | UTC ISO-8601. Set to `created_at` on create. Set to the current time when an administrator edits the item. Confirm and revoke do not change it. 最後更新時間. |

### Validation

- Create and edit: if the trimmed title is empty, do not write, return HTTP 400 with `field: title`. The UI shows a field error and stays on the form.
- Description may be omitted; the server stores `''`.

## Personal confirmation

One status for one display name on one work item. Independent of every other person and of title and description.

| Field | Storage | Rules |
|-------|---------|--------|
| displayName | `confirmation.display_name` | Trimmed name. Part of the primary key. |
| workItemId | `confirmation.work_item_id` | FK to `work_item.id` ON DELETE CASCADE. Part of the primary key. |
| status | implied | A row means 已確認 (`confirmed`). No row means 待確認 (`pending`). |

### State transitions

| From | Action | To | Who |
|------|--------|----|-----|
| no row (待確認) | Confirm | row inserted (已確認) | The signed-in display name |
| row (已確認) | Confirm again | row unchanged (已確認) | Same person. Still counted in the success total. |
| row (已確認) | Revoke | row deleted (待確認) | Same person |
| no row (待確認) | Revoke | stays absent | Allowed as a no-op if called; the UI only offers 撤銷確認 when the row exists |
| any | Admin edits title or description | confirmation rows unchanged | Administrator |
| any | Admin deletes the work item | confirmation rows deleted with the item | Administrator |

A confirm request that includes an id with no work item writes nothing and returns 404. The same rule applies to revoke, edit, and delete of a missing id.

New items have no confirmation rows, so every person sees 待確認.

## Relationships

```text
work_item 1 --- * confirmation
confirmation.display_name  -->  Person (display name, not a table)
```

## List query

- Filter: none (no search).
- Order: `created_at DESC, id DESC` by default. Ascending uses `created_at ASC, id ASC`.
- Page size: 10. `page` is 1-based. Page past the end returns an empty `items` array and the real `total`.
- User list joins confirmation for the request's display name and reports `pending` when no row exists.
- Admin list does not include a personal status.

## Schema

```sql
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS work_item (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  title TEXT NOT NULL,
  description TEXT NOT NULL DEFAULT '',
  created_at TEXT NOT NULL,
  updated_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS confirmation (
  display_name TEXT NOT NULL,
  work_item_id INTEGER NOT NULL,
  PRIMARY KEY (display_name, work_item_id),
  FOREIGN KEY (work_item_id) REFERENCES work_item(id) ON DELETE CASCADE
);
```
