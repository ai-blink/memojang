# Dev Progress

## Current

2026-07-06: Shell chrome polish is in progress. The app now has a custom tab-like title bar, compact localized menu row, lightweight Markdown/editor toolbar, improved dark/light menu contrast, and a startup guard for missing `WINDIR`/`WinDir` environment variables in harness-launched WPF sessions. Privacy manual capture matrix checks remain pending.

## Completed

- 2026-07-06: Initial Git repository created and pushed to `origin/main` with `README.md` in commit `e01bc2f`.
- 2026-07-06: Program plan saved at `notes/plans/2026-07-06-memojang-program-plan.md`; UX structure, secret scan, and planning-artifact shape passed after init normalization.
- 2026-07-06: `.NET 10 WPF` solution scaffolded in commit `8e3ab8e feat(project): scaffold Memojang WPF app`; build and tests passed.
- 2026-07-06: Minimal editor shell implemented and verified with `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln`.
- 2026-07-06: Basic file commands and line-ending detection implemented and verified with `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln`.
- 2026-07-06: Dirty-document discard prompts implemented and verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln`, and a process-start smoke run.
- 2026-07-06: Markdown preview foundation implemented and verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln`, and a process-start smoke run.
- 2026-07-06: Markdown table helper and capture exclusion foundation implemented and verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln`, and a process-start smoke run.
- 2026-07-06: V1 preview folder publish configured with `PreviewFolder.pubxml`, documented in `docs/PREVIEW_BUILD.md`, and verified by running the published executable.
- 2026-07-06: Capture exclusion diagnostics improved and verified with `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln`.
- 2026-07-06: Basic Notepad-like shortcuts implemented and verified with `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln`.
- 2026-07-06: Find panel and wraparound search implemented and verified with `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln`.
- 2026-07-06: Replace current/all added to the find panel and verified with `dotnet build .\Memojang.sln` and `dotnet test .\Memojang.sln`.
- 2026-07-06: Localization and theme foundation added for supported UI languages, with English fallback, Arabic RTL flow, and system/light/dark resource dictionaries.
- 2026-07-06: Custom title/tab chrome, compact menus, menu popup contrast fixes, and a small toolbar were added; verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln`, and app launch smoke.

## Next

- Run manual Windows capture matrix checks for privacy mode and record observed tool behavior.
- Tighten toolbar commands so visible controls map to implemented Markdown helpers only.

## Blockers

- No active blockers.
