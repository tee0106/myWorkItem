# Feature Specification: Work Item List

**Feature Branch**: `001-work-item-list`

**Created**: 2026-10-14

**Status**: Draft

**Input**: User description: "Please make Work Item List webpage, and have both User and Admin feature that record in requirments.md ."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sign in as a front-end user or an administrator (Priority: P1)

A person chooses Front-end user or Administrator, enters a display name, and reaches the matching area. The same name always brings back that person's own confirmation history.

**Why this priority**: Every later action is stored against a person. Without a stable identity, personal confirmation cannot be separated between people.

**Independent Test**: Sign in twice with the same name and once with a different name. The first name keeps its own statuses; the second name does not see them.

**Acceptance Scenarios**:

1. **Given** the person has not signed in, **When** they open the application, **Then** they must choose Front-end user or Administrator and enter a display name before any work item list is shown.
2. **Given** a person signs in as a Front-end user, **When** the list opens, **Then** they can view and confirm items and they cannot add, edit, or delete items.
3. **Given** a person signs in as an Administrator, **When** they enter the admin area, **Then** they can add, edit, and delete work items.
4. **Given** two people use different display names, **When** each confirms items, **Then** each person sees only their own confirmation status.
5. **Given** a person enters a display name with spaces at the ends, **When** they sign in, **Then** the name without those spaces is the same person on the next visit.

---

### User Story 2 - View the work item list (Priority: P1)

A front-end user opens the list to see which work items exist, in which order, and with their own status.

**Why this priority**: The list is the home of the product. An empty list must still explain that there is nothing to do.

**Independent Test**: Open `/work-items` with no items, then again after an administrator has added items, and switch the sort direction.

**Acceptance Scenarios**:

1. **Given** a signed-in front-end user, **When** they open `/work-items`, **Then** they see a table or list with 編號, 標題, and 狀態 for each item.
2. **Given** no work items exist, **When** the user opens `/work-items`, **Then** the page shows 「目前無待辦項目」.
3. **Given** several items exist, **When** the list first loads, **Then** items are ordered by created time, newest first.
4. **Given** the list is showing newest first, **When** the user switches sort direction, **Then** the same items show oldest first, and switching again restores newest first.
5. **Given** more items exist than one page can show, **When** the user moves to another page, **Then** only that page of items is shown and their statuses stay correct.

---

### User Story 3 - Confirm and revoke items (Priority: P1)

A front-end user checks one or more items and confirms them, or returns a confirmed item to 待確認. The change applies only to that user.

**Why this priority**: Personal confirm and revoke is the main reason the list exists.

**Independent Test**: Check items, confirm them, revoke one, and sign in as someone else to see that their statuses did not change.

**Acceptance Scenarios**:

1. **Given** items are listed, **When** the user checks a row, **Then** that row is visibly highlighted and its checkbox is checked.
2. **Given** no row is checked, **When** the user looks at 確認, **Then** the button is disabled.
3. **Given** at least one displayed row is checked, **When** the user looks at 確認, **Then** the button can be clicked.
4. **Given** the user checks the header checkbox, **When** the page has several displayed items, **Then** every item on the current page is selected. Using it again clears those selections.
5. **Given** one or more displayed items are checked, **When** the user clicks 確認 and the save succeeds, **Then** those items show 已確認, every checkbox is cleared, and the page shows 「已成功確認 X 項目」 where X is how many items were confirmed.
6. **Given** the user clicks 確認, **When** the save fails, **Then** an error is shown and every item keeps its previous status.
7. **Given** a row's status is 已確認, **When** the list is shown, **Then** that row shows 撤銷確認. A row that is 待確認 does not show it.
8. **Given** the user clicks 撤銷確認, **When** the dialog opens, **Then** it says 「確定要將此項目標記回『待確認』嗎？」 and offers 取消 and 確定.
9. **Given** the revoke dialog is open, **When** the user chooses 取消, **Then** the status stays 已確認 and the dialog closes.
10. **Given** the user chooses 確定 and the save succeeds, **When** the list updates, **Then** that row shows 待確認, 撤銷確認 is hidden, and the page shows 「已將 X 項目標記為待確認」 where X is 1.
11. **Given** the user chooses 確定, **When** the save fails, **Then** an error is shown and the row stays 已確認.
12. **Given** an item's 最後更新時間 is already shown, **When** the user confirms or revokes that item, **Then** 最後更新時間 stays the same.

---

### User Story 4 - Keep confirmation after returning (Priority: P2)

A front-end user closes the page or signs in again later and still sees the confirmations they already made.

**Why this priority**: Confirmation is only useful if it is still there on the next visit.

**Independent Test**: Confirm an item, refresh the page, then close the browser, sign in again with the same name, and check the status.

**Acceptance Scenarios**:

1. **Given** a user has confirmed an item, **When** they refresh `/work-items` or sign in again with the same name, **Then** that item still shows 已確認 and every checkbox starts unchecked.
2. **Given** a user has never confirmed an item, **When** they open the list, **Then** that item shows 待確認.
3. **Given** user A has confirmed an item and user B has not, **When** user B opens the list, **Then** user B sees 待確認 for that item.

---

### User Story 5 - View work item detail (Priority: P2)

A front-end user opens one item to read its description and times, then returns to the same place in the list.

**Why this priority**: The list is a summary. The detail page holds the description and timestamps.

**Independent Test**: From a sorted, paged list, open one item and use 返回列表.

**Acceptance Scenarios**:

1. **Given** a list row, **When** the user chooses 查看詳情, **Then** the app opens `/work-items/{id}`.
2. **Given** the detail page for an existing item, **When** it loads, **Then** it shows 編號, 標題, 描述, 建立時間, the signed-in user's 狀態, and 最後更新時間.
3. **Given** the user opened detail from a specific page and sort direction, **When** they choose 返回列表, **Then** the list shows that same page and sort direction, with checkboxes unchecked.
4. **Given** the address does not match an existing item, **When** the detail page loads, **Then** the user sees that the item was not found and can return to the list.

---

### User Story 6 - Add a work item (Priority: P2)

An administrator adds a work item so every front-end user can see it as 待確認.

**Why this priority**: Users can only confirm items an administrator has added.

**Independent Test**: Open the new-item page, try a blank title, then save a valid title and find the item at the top of a newest-first list.

**Acceptance Scenarios**:

1. **Given** an administrator, **When** they open `/admin/work-items/new`, **Then** the form has 標題 (required) and 描述 (optional).
2. **Given** the title is empty or only spaces, **When** the administrator tries to submit, **Then** the form shows a field error, stays on the page, and no item is created.
3. **Given** a non-empty title, **When** the administrator submits and the save succeeds, **Then** they return to the admin list, see 「新增成功」, and the new item is first when the list is newest first.
4. **Given** a new item was added, **When** any front-end user opens the list, **Then** that item appears as 待確認 for them.

---

### User Story 7 - Edit a work item (Priority: P3)

An administrator corrects a title or description. People's existing confirmations stay as they were.

**Why this priority**: Titles and descriptions change, but a correction must not wipe personal progress.

**Independent Test**: Edit an item that one user has confirmed, save, and check both the new wording and that user's status.

**Acceptance Scenarios**:

1. **Given** the admin list, **When** the administrator chooses 編輯 on a row, **Then** `/admin/work-items/{id}/edit` opens with the current title and description filled in.
2. **Given** the title is cleared, **When** the administrator tries to save, **Then** a field error is shown and the previous title and description remain.
3. **Given** a valid title, **When** the administrator clicks 儲存 and the save succeeds, **Then** they return to the admin list, see 「更新成功」, and the row shows the new title and description.
4. **Given** a user had already confirmed that item, **When** the administrator saves a new title, **Then** that user still sees 已確認 on the renamed item.

---

### User Story 8 - Delete a work item (Priority: P3)

An administrator removes an item that should no longer appear for anyone.

**Why this priority**: The list should not keep obsolete items, and deletion must be deliberate.

**Independent Test**: Start a delete, cancel, then delete the same item and confirm it is gone for a front-end user.

**Acceptance Scenarios**:

1. **Given** the admin list, **When** the administrator clicks 刪除, **Then** a dialog asks 「確定要刪除此項目嗎？」.
2. **Given** the delete dialog is open, **When** the administrator cancels, **Then** the item remains.
3. **Given** the administrator confirms deletion and it succeeds, **When** the list updates, **Then** the row is gone and the page shows 「刪除成功」.
4. **Given** deletion fails, **When** the result is shown, **Then** an error appears and the row remains.
5. **Given** an item was deleted, **When** a front-end user opens the list or its old detail address, **Then** the item is no longer in the list and the old detail address is treated as not found.
6. **Given** two items with different 編號 values, **When** one item is deleted, **Then** the remaining item keeps its original 編號.

---

### Edge Cases

- The header checkbox selects and clears only the items on the current page. Items on other pages are not selected.
- Changing page or sort direction clears every checkbox. 確認 becomes disabled until the user checks a displayed row again.
- Selecting some but not all displayed rows leaves the header checkbox in a mixed state. Activating it then selects every displayed row.
- A title that is empty or only spaces is rejected on both create and edit.
- Duplicate titles are allowed. Items are told apart by 編號.
- A new item's 最後更新時間 matches its 建立時間 until an administrator edits it. Confirm and revoke do not change 最後更新時間.
- If the item disappears before confirm, revoke, or save finishes, the action fails, an error is shown, and the list is shown again without that item.
- Refreshing the list never restores checkbox selections. Only confirmation status is remembered.
- A front-end user who opens an admin address is refused and does not see add, edit, or delete controls.
- 描述 may be blank. The detail page still shows the description field.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A person MUST choose Front-end user or Administrator and enter a display name before any work item list is shown.
- **FR-002**: The system MUST treat a trimmed display name as that person's identity, and MUST keep confirmation status separate for each name.
- **FR-003**: A front-end user MUST be able to open `/work-items` and see 編號, 標題, and 狀態 for each work item.
- **FR-004**: When no work items exist, `/work-items` MUST show 「目前無待辦項目」.
- **FR-005**: The list MUST default to created time, newest first, and the user MUST be able to switch between newest first and oldest first.
- **FR-006**: The list MUST show 10 items per page when more than 10 items exist.
- **FR-007**: Each displayed row MUST have a checkbox, and a checked row MUST be visibly highlighted.
- **FR-008**: The header checkbox MUST select or clear only the items displayed on the current page.
- **FR-009**: 確認 MUST stay disabled until at least one displayed row is checked, and MUST be enabled when one or more are checked.
- **FR-010**: A successful confirm MUST set the signed-in user's selected items to 已確認, clear all checkboxes, and show 「已成功確認 X 項目」, where X is the number of items confirmed.
- **FR-011**: A failed confirm MUST show an error and MUST leave every item's previous status unchanged.
- **FR-012**: 撤銷確認 MUST appear only on rows whose status for the signed-in user is 已確認.
- **FR-013**: 撤銷確認 MUST ask 「確定要將此項目標記回『待確認』嗎？」 and MUST offer 取消 and 確定. 取消 MUST make no change.
- **FR-014**: A successful revoke MUST set that user's status for that item to 待確認, hide 撤銷確認, and show 「已將 X 項目標記為待確認」, where X is 1.
- **FR-015**: A failed revoke MUST show an error and MUST leave the status as 已確認.
- **FR-016**: After a refresh or a later sign-in with the same name, previously confirmed items MUST still show 已確認, and all checkboxes MUST start unchecked.
- **FR-017**: One person's confirm or revoke MUST NOT change any other person's status for the same item.
- **FR-018**: A new work item MUST start as 待確認 for every person.
- **FR-019**: Each list row MUST provide 查看詳情, which opens `/work-items/{id}`. Checkbox, 確認, and 撤銷確認 MUST NOT open the detail page.
- **FR-020**: The detail page MUST show 編號, 標題, 描述, 建立時間, the signed-in user's 狀態, and 最後更新時間.
- **FR-021**: 返回列表 MUST restore the list page and sort direction the user had before opening the detail, with checkboxes unchecked.
- **FR-022**: An unknown item address MUST say the item was not found and MUST offer a way back to the list.
- **FR-023**: Only an Administrator MUST be able to add, edit, or delete work items. A front-end user MUST be refused those actions.
- **FR-024**: `/admin/work-items/new` MUST collect 標題 as required and 描述 as optional.
- **FR-025**: Create and edit MUST reject an empty or whitespace-only title, show a field error, and MUST NOT save.
- **FR-026**: A successful create MUST return to the admin list, show 「新增成功」, and place the new item first when sorted newest first.
- **FR-027**: `/admin/work-items/{id}/edit` MUST open with the current title and description filled in.
- **FR-028**: A successful save MUST return to the admin list, show 「更新成功」, show the new title and description, and MUST keep each person's existing confirmation status.
- **FR-029**: 刪除 MUST ask 「確定要刪除此項目嗎？」. Canceling MUST keep the item.
- **FR-030**: A successful delete MUST remove the item for every person and show 「刪除成功」. A failed delete MUST show an error and keep the item.
- **FR-031**: Changing page or sort direction MUST clear checkboxes so 確認 applies only to items the user can currently see.
- **FR-032**: 編號 MUST be a stable unique number assigned when the item is created, and MUST NOT be reused or renumbered after another item is deleted.
- **FR-033**: 最後更新時間 MUST change when an administrator creates or edits the shared item, and MUST NOT change when a user confirms or revokes.

### Key Entities

- **Person**: A display name plus a role of Front-end user or Administrator. The name is how personal status is remembered.
- **Work Item**: A shared item with 編號, 標題, 描述, 建立時間, and 最後更新時間. All signed-in users see the same item content.
- **Personal confirmation**: One status, 待確認 or 已確認, for one person on one work item. It is independent of every other person and of the shared title and description.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A signed-in front-end user can tell whether the list has items, or can read 「目前無待辦項目」, within 1 minute of opening `/work-items`.
- **SC-002**: A user can check the items on the current page and complete 確認 without leaving the list. On success, those rows show 已確認 and the message includes the correct count.
- **SC-003**: After the browser is closed and the same person signs in again, 100% of items they had confirmed still show 已確認, and no checkbox is checked.
- **SC-004**: In a check with two different display names, each person sees only their own status for every shared item.
- **SC-005**: An administrator can add an item with a title and see 「新增成功」 and the new item at the top of a newest-first list in under 1 minute.
- **SC-006**: Submitting a blank title on create or edit never creates or changes an item, and a field error is visible in every such attempt.
- **SC-007**: Canceling a revoke dialog or a delete dialog leaves the item and its statuses unchanged in 100% of attempts.
- **SC-008**: From a list page other than the first page, opening a detail and choosing 返回列表 restores that same page and sort direction.
- **SC-009**: A person can move through a list of 100 items by paging, and changing sort direction does not change any person's confirmation status.

## Assumptions

- The product is used locally by a small group. Public hosting, passwords, email invites, and external sign-in are out of scope. A display name is enough to tell people apart.
- The same person uses the same display name each time. Leading and trailing spaces are ignored. Two different spellings are two people.
- The admin list is at `/admin/work-items`. After create, edit, or delete, the administrator returns there.
- An administrator manages shared items. Personal confirmation is recorded only for a Front-end user sign-in. An administrator who also wants a personal list signs in again as a Front-end user with a display name.
- Page size is 10. Select-all and 確認 apply to the current page only.
- Sort is by created time only. No search, filters, due dates, assignees, attachments, comments, or bulk revoke are included.
- Duplicate titles are allowed. 編號 is the stable identifier people see.
- Times are shown in a readable local date-and-time form. 最後更新時間 starts equal to 建立時間.
- The list uses a 查看詳情 control rather than making the whole row open the detail page, so checkbox clicks stay on the list.
- On-screen messages use the Chinese wording in these requirements, including 「目前無待辦項目」, 「已成功確認 X 項目」, 「已將 X 項目標記為待確認」, 「新增成功」, 「更新成功」, and 「刪除成功」.
- If a save fails because the item was already deleted, the user sees an error and the current list without that item.
