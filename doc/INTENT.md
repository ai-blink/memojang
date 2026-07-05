# Memojang Intent

## Product Target

Build a Windows 11-first Notepad-like editor in .NET 10 WPF that opens and edits plain text quickly, shows Markdown and Markdown tables beautifully, supports lightweight document toolbar actions, and can exclude the app window from common screen/image capture paths when privacy mode is enabled.

## Current Blocker

The repository has only an initial README and a planning artifact. It needs the app skeleton and live project docs before implementation can proceed coherently.

## Acceptance Shape

V1 is acceptable when a user can open or create a text/Markdown document, edit it with Notepad-like keyboard/menu expectations, render and edit Markdown tables without corrupting source text, toggle capture exclusion, see honest protection status, and save with predictable encoding and line-ending behavior.

## Non-Goals For V1

- No cloud sync or collaboration.
- No AI writing/summarization.
- No Microsoft Notepad branding, icon, private resources, or exact asset copying.
- No full WYSIWYG word processor or spreadsheet replacement.

## Primary References

- Program plan: `notes/plans/2026-07-06-memojang-program-plan.md`
- Live roadmap: `rules/dev-roadmap.md`
- Architecture rules: `rules/dev-arch.md`
- UX rules: `rules/dev-ux.md`

## Verification Spine

- Automated checks: `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln` once code exists.
- Manual checks: Notepad-like menu/shortcut comparison, Markdown table fixture editing, Snipping Tool/OBS/Recall capture matrix.
- Documentation checks: `git diff --check` and app-dev-workflow secret scan for docs-only changes.
