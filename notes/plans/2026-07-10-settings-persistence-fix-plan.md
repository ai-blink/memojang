# Settings Persistence Fix Plan

Date: 2026-07-10

## Finish Line

Restarting Memojang retains the ordered recent-file list and the user's capture-exclusion selection.

## Acceptance Checks

- Settings round-trip tests cover recent files and capture exclusion.
- Invalid settings fall back without preventing startup.
- `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln` pass from an isolated output path.
- `git diff --check` passes.

## Scope Limit

Do not persist open tabs, unsaved document contents, editor selection, or window geometry in this fix.

## Review Budget

At most two implementation/fix loops.

## Stop Rule

Stop after automated acceptance checks pass and the live progress and roadmap documents record the fix.
