# Memojang

Memojang is a Windows 11-first text and Markdown editor that keeps the familiar Notepad feel while adding polished Markdown/table editing and optional window capture exclusion.

> This file is the single canonical project instruction entrypoint. Do not add a competing authoritative `AGENTS.md`; if compatibility is ever needed, make it a thin pointer to this file.

## Absolute Rules

- Keep plans focused on intent, constraints, acceptance checks, and verification; put implementation detail in scoped plans or source files.
- Keep source files under 500 lines per responsibility and live docs concise; split by role before appending large sections.
- Anchor non-trivial work to `doc/INTENT.md`, `rules/dev-roadmap.md`, and the current plan artifact.
- Preserve the Notepad-like product target: lightweight Windows UX first, Markdown/table support second, larger editor-suite ideas later.

## Folder Map

- `src/` - future .NET 10 WPF application source.
- `tests/` - future automated tests and smoke probes.
- `samples/` - future Markdown/text fixtures for manual and automated verification.
- `doc/` - stable project intent and workflow templates.
- `docs/` - architecture notes and diagram index.
- `rules/` - live development state, roadmap, decisions, context, and UX rules.
- `notes/` - planning, transfer, and run artifacts.

## Index

- Product intent and acceptance shape: `doc/INTENT.md`
- Current roadmap: `rules/dev-roadmap.md`
- Current progress and blockers: `rules/dev-progress.md`
- Architecture and file ownership: `rules/dev-arch.md`
- UX rules: `rules/dev-ux.md`
- Decisions: `rules/dev-decisions.md`
- Resume snapshot: `rules/dev-context.md`
- Diagram index: `docs/diagrams/README.md`
- Initial program plan: `notes/plans/2026-07-06-memojang-program-plan.md`

## Commands

```powershell
dotnet --info
dotnet build .\Memojang.sln
dotnet test .\Memojang.sln
```

These commands become required once the solution exists. Before that, validate docs with `git diff --check` and the app-dev-workflow secret scan.

## Completion Discipline

Current product target: a .NET 10 WPF Windows 11 Notepad-like editor with Markdown/table editing and honest capture-exclusion status.

For non-trivial work:
- Define `Finish Line`, `Acceptance Checks`, `Scope Limit`, `Review Budget`, and `Stop Rule` before implementation.
- Fix only `BLOCKER` issues that prevent the finish line; record `FOLLOW_UP` and `IGNORE_FOR_V1` separately.
- Use at most two fix/review loops unless the user explicitly approves more.
- Stop after acceptance checks pass and update `rules/dev-progress.md` and `rules/dev-roadmap.md`.
- Subagents may execute bounded, verifiable slices only; the lead agent owns scope, integration, and final verification.
- Do not copy Microsoft Notepad branding, icons, strings, or resources; build a compatible-feeling Memojang UI instead.

## Boundaries

- Do not turn V1 into a full Word/Excel-style editor; keep rich editing limited to Markdown helpers and table support.
- Do not promise absolute DRM; capture exclusion must show success/failure and document unsupported capture paths.
- Do not import code from reference Notepad clones without checking license and attribution; prefer behavioral reference only for V1.
