<!--
Sync Impact Report
- Version change: (none) → 1.0.0
- Modified principles: none (initial ratification)
- Added sections:
  - Core Principles (I–V)
  - Technical Constraints
  - Quality Gates
  - Governance
- Removed sections: none
- Templates:
  - .specify/templates/plan-template.md — ⚠ pending (file absent; no Constitution Check to align)
  - .specify/templates/spec-template.md — ⚠ pending (file absent)
  - .specify/templates/tasks-template.md — ⚠ pending (file absent; testing tasks cannot be added to a missing template)
  - .specify/templates/commands/*.md — ⚠ pending (directory absent)
  - .cursor/commands/*.md — ✅ reviewed; no agent-only names such as CLAUDE
  - README.md — ✅ no principle references to update
  - .cursor/rules/specify-rules.mdc — ✅ no principle references to update
- Follow-up TODOs: none
-->

# myWorkItem Constitution

## Core Principles

### I. Code Quality

Production code MUST be explicit, typed, and consistent with the stack in `requirments.md`.

- Frontend code MUST use Vue 3, TypeScript strict mode, the Composition API, and composables.
- Backend code MUST use .NET 9. Data access MUST use Dapper. Entity Framework MUST NOT be added.
- Public functions, API contracts, and composable return values MUST declare their types.
- Identifiers MUST use Work Item domain language (title, status, confirmation).
- A change is complete only when the frontend typecheck and `dotnet build` both succeed.
- UI MUST use existing DaisyUI components before a custom widget is introduced.

Rationale: Typed contracts and one UI kit keep a two-project app reviewable by a single reader.

### II. Testing

User-visible behavior MUST be proven by automated tests before the work is treated as done.

- Each acceptance path in `requirments.md` MUST have a test that fails when that path breaks.
- Tests MUST cover list order and the empty state, per-user confirm and revoke, status after reload, detail fields, and admin create, edit, and delete, including validation failure.
- Tests MUST assert observable results: status text, the disabled Confirm control, toasts, and dialog outcomes.
- A bug fix MUST add or extend a test that fails on the bug and passes after the fix.
- Backend tests MUST run against SQLite. Frontend tests MUST exercise page behavior, not private fields.

Rationale: Confirmation status is stored per user. Tests are how we prove one user cannot overwrite another.

### III. Maintainability

Each concern MUST live in one place so a later change has a single edit site.

- The repository MUST keep `frontend/` and `backend/` as separate projects.
- SQL MUST live in data-access code. Controllers MUST NOT contain SQL.
- Selection and confirmation UI state MUST live in composables. Components MUST NOT talk to the database.
- Work Item field names MUST be defined once per layer and reused by list, detail, and admin forms.
- Unused dependencies, dead code, and commented-out blocks MUST NOT be committed.

Rationale: Personal status, shared items, and admin edits stay correct only when their code does not leak across layers.

### IV. Complete Changes

A change MUST finish the success, empty, and failure paths the requirements name for the behavior it touches.

- Confirm stays disabled until at least one row is checked. A failed update MUST leave the prior status in place.
- Admin title validation MUST block submit and show a field error when the title is empty.
- Delete and revoke MUST require an explicit confirm dialog and MUST make no change on cancel.
- New behavior MUST NOT ship with a known broken path left for later.

Rationale: The requirements define the failure and empty paths as part of the feature, not as polish.

### V. Simplicity

Add a library, layer, or pattern only when a current requirement cannot be met without it.

- Do not add a second ORM, a global state library, or a UI kit besides DaisyUI and Tailwind.
- Duplicate a short block until the same logic appears in a third place; then extract it.
- Authentication and persistence MUST stay only as large as per-user status requires.

Rationale: This is a local CRUD app. Extra structure costs more than the requirements justify.

## Technical Constraints

- Frontend: Vue 3, TypeScript strict, Composition API, composables, DaisyUI on Tailwind CSS.
- Backend: .NET 9 Web API with Dapper. No Entity Framework.
- Database: a local SQLite file.
- Run locally with `npm run dev` and `dotnet run`. Hosted deployment is out of scope.
- Routes and copy in `requirments.md` are binding, including `/work-items`, `/work-items/{id}`, `/admin/work-items/new`, and `/admin/work-items/{id}/edit`.

## Quality Gates

- Plans MUST include a Constitution Check that maps each principle above to the proposed design.
- A gate fails when a MUST in this file is violated and the plan records no written exception.
- Tasks for a feature MUST include tests for the acceptance paths that feature changes.
- Implementation MUST NOT start while a Constitution Check gate is failing.

## Governance

This constitution overrides conflicting local habits. Product behavior in `requirments.md` stays binding where it does not weaken a MUST in this file.

Amendments:

- Edit `.specify/memory/constitution.md`, record the change in the Sync Impact Report, and bump the version.
- MAJOR: remove or redefine a principle. MINOR: add a principle or expand a MUST. PATCH: wording that does not change a rule.
- When plan, spec, or task templates exist, propagate principle changes into them. Update agent guidance that cites a principle.

Compliance:

- Every implementation plan MUST pass the Constitution Check before tasks are generated.
- Review MUST reject a change that breaks a MUST unless the plan records an exception and the constitution version it was checked against.
- Runtime guidance lives in `.cursor/rules/specify-rules.mdc`. When it disagrees with this file on quality rules, this file wins.

**Version**: 1.0.0 | **Ratified**: 2026-10-14 | **Last Amended**: 2026-10-14
