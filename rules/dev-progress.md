# Dev Progress

## Current

2026-07-06: UI localization and theme foundations added. Startup now selects a supported UI language from the Windows UI culture with English fallback, applies RTL flow for Arabic, and applies a system/light/dark theme resource structure. Privacy manual capture matrix checks remain pending.

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
- 2026-07-06: Localization and theme foundation added for Korean, English, Japanese, Hindi, Chinese, French, Arabic, Spanish, Bengali, Portuguese, Russian, and Indonesian; verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln`, `dotnet publish .\src\Memojang.App\Memojang.App.csproj -p:PublishProfile=PreviewFolder`, published exe smoke, and `git diff --check`.

## Next

- Run manual Windows capture matrix checks for privacy mode and record observed tool behavior.
- Add a small persistent settings slice for theme/language override after the manual privacy matrix.

## Blockers

- No active blockers.
