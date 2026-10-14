# Quickstart: Work Item List

Validation guide for the local app. Behavior is specified in [spec.md](./spec.md). Shapes are in [data-model.md](./data-model.md) and [contracts/openapi.yaml](./contracts/openapi.yaml).

## Prerequisites

- .NET 9 SDK
- Node.js with npm
- Repository root as the working directory

## Run

1. Start the API from `backend/WorkItem.Api`:

   ```text
   dotnet run
   ```

   The API creates `backend/data/workitems.db` if it is missing and applies `schema.sql`.

2. Start the UI from `frontend`:

   ```text
   npm install
   npm run dev
   ```

   The dev server proxies `/api` to the API.

3. Open the Vite URL. A person who is not signed in sees the sign-in form before any list.

## Checks

Run these before treating the feature as done:

```text
cd frontend
npm run typecheck
npm test

cd backend/WorkItem.Api.Tests
dotnet test

cd backend/WorkItem.Api
dotnet build
```

## Scenarios

Use two display names, `A` and `B`. Sign in as Front-end user unless the step says Administrator.

| Step | Action | Expected |
|------|--------|----------|
| 1 | Open the app signed out | Sign-in form. No work item list. |
| 2 | Sign in as `A`, open `/work-items` with an empty database | 「目前無待辦項目」 |
| 3 | Sign in as Administrator, open `/admin/work-items/new`, submit a blank title | Field error. Stay on the form. No new row. |
| 4 | Submit title `First` and a description | Back on `/admin/work-items`. 「新增成功」. The row is first when newest-first. |
| 5 | Add `Second` | `Second` is above `First` when newest-first. Switch sort: `First` is above `Second`. |
| 6 | As front-end user `A`, open `/work-items` | Both rows show 待確認. 確認 is disabled. Checkboxes are unchecked. |
| 7 | Check both rows on the page and press 確認 | Both show 已確認. Checkboxes clear. Toast 「已成功確認 2 項目」. |
| 8 | Refresh, then sign out and sign in again as `A` | Both still 已確認. Checkboxes unchecked. |
| 9 | Sign in as `B` | Both show 待確認. |
| 10 | As `A`, press 撤銷確認 and choose 取消 | Status stays 已確認. |
| 11 | Press 撤銷確認 and choose 確定 | That row shows 待確認. 撤銷確認 is hidden. Toast 「已將 1 項目標記為待確認」. |
| 12 | Open 查看詳情 from page 2 of a newest-first list (add items until two pages exist) | `/work-items/{id}` shows 編號, 標題, 描述, 建立時間, that user's 狀態, 最後更新時間. |
| 13 | Press 返回列表 | Same page and sort. Checkboxes unchecked. |
| 14 | Open a missing id | Not-found message and a way back to the list. |
| 15 | As Administrator, edit an item `A` has confirmed. Clear the title and save. | Field error. Title unchanged. |
| 16 | Save a new title | 「更新成功」. New title on the row. `A` still sees 已確認. 最後更新時間 changes. Confirm and revoke do not change it. |
| 17 | Press 刪除 and cancel | Row remains. |
| 18 | Press 刪除 and confirm | Row gone. 「刪除成功」. The other item keeps its 編號. `A` no longer sees the item. The old detail address is not found. |
| 19 | As front-end user, open `/admin/work-items` | Refused. No add, edit, or delete controls. API admin calls return 403. |

Header select-all selects only the current page. Changing page or sort clears every checkbox and disables 確認. A failed confirm, revoke, or delete shows an error and leaves the previous status or row in place.
