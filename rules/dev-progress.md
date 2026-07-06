# Dev Progress

## Current

2026-07-07: Document mode, Markdown image support, and visible UI feedback passes are in progress-complete for the V1 slice. The app detects `Text` vs `Markdown`, shows document mode in the status bar, debounces Markdown preview updates, previews local Markdown image syntax with missing-image placeholders, can copy inserted Markdown images into a sidecar assets folder, localizes the visible Markdown/capture-exclusion toolbar labels in Korean, wires basic Markdown toolbar buttons, applies themed scrollbars, keeps preview text readable in dark theme, renders Markdown thematic breaks, fenced code blocks, unordered list markers, bold emphasis, and empty-heading edge cases, and reduces tab/menu border separation toward the Windows Notepad target. Verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln`, app launch smoke, and `git diff --check`. Privacy manual capture matrix checks remain pending.

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
- 2026-07-07: External document editor image/preview research and V1 plan added; Markdown document mode, debounced preview, image preview, sidecar asset copy/link insertion, `.txt` image blocking, and focused tests were implemented and verified.
- 2026-07-07: User-visible toolbar polish applied: Korean labels now avoid stray English in the Korean UI, privacy wording is now security-oriented, basic Markdown toolbar buttons perform edits, and the tab/menu chrome is visually flatter.
- 2026-07-07: Preview/toolbar feedback pass applied: dark preview foreground follows the app theme, scrollbars use themed styling, capture exclusion is exposed as a left/right toolbar toggle, bold Markdown emphasis is rendered, `skillhooks` emphasis is blue, and `***` renders as a separator block.

## Next

- Run manual Windows capture matrix checks for privacy mode and record observed tool behavior.
- Manually check saved `.md` drag/drop image insertion and `.txt` image blocking in a visible desktop session.

## Blockers

- No active blockers.
