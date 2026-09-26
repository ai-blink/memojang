# Dev Progress

## Current

2026-09-27: Translation review fixes applied.
- Three subagent reviews (ja/zh/id, es/fr/pt/ru, ar/hi/bn) found mostly access-key collisions. Some were inherited from the English source (New tab/New text = T, Cut/Time-Date = T, Paste/Find previous = P, context Cut/Table = T); others came from translation (worst: ru, fr, id).
- `AccessKeyCollisionTests` now checks every menu group and the editor context menu for all 12 resource sets, and all collisions are fixed (en: New t_ext, Find pre_vious, Time/_Date, Ta_ble).
- Content fixes:
  - ar: capture-exclusion term aligned, and "deleted" corrected to "excluded" in the limited-mode detail.
  - hi: capture-exclusion menu term aligned.
  - hi and bn: dark-mode term aligned between menu and toolbar.
  - zh: full-width question mark.
  - id: shorter capture strings, "pengodean", and a grammar fix.
- One review claim was wrong (zh already used curly quotes) and was not applied. Low-confidence suggestions were skipped.
- Verified with the isolated `dotnet build` (0 warnings/errors) and 134 passing tests.

2026-09-27: All 11 non-English locales are fully translated.
- ar, bn, es, fr, hi, id, ja, pt, ru, and zh went from 15 of 132 keys to all 132.
- `MenuPrivacy` was corrected from "privacy" to "capture exclusion" in 10 locales, and missing accents were fixed in pt/es/fr.
- `LocalizationCompletenessTests` now fails if any locale misses a key or diverges in `{n}` placeholders or `_` access keys.
- Verified with the isolated `dotnet build` (0 warnings/errors) and 122 passing tests. Translation quality and Arabic RTL rendering have not been reviewed by a person.

2026-09-27: Session restore (hot exit) was added.
- On close, open tabs and unsaved text are written to `%LOCALAPPDATA%\Memojang\session.json` (write-then-move) instead of prompting. The next start restores them.
- Clean file tabs store only the path and are re-read from disk. User files are never auto-saved.
- If the session write fails, closing falls back to the save prompts.
- Settings > "Restore tabs and unsaved text on start" turns it off, and the session file is then cleared on close.
- A startup file opens in a new tab, or re-activates its existing tab, so it never overwrites a restored tab.
- Verified with the isolated `dotnet build` (0 warnings/errors), 111 passing tests, and a real-app check: typing text and closing exited without a prompt; the relaunch restored "hot exit test" and logged `Session restored tabs=1`. The test session file was removed afterward.

2026-09-27: Line endings are preserved while editing, and the status bar has encoding and line-ending menus.
- Enter and paste use the document's line ending, so an LF file no longer gains CRLF (WPF defaults to CRLF).
- Clicking the encoding status picks the save encoding (UTF-8, UTF-8 BOM, UTF-16 LE/BE, ANSI).
- Clicking the line-ending status converts the whole text as one undo step (`LineEnding.Normalize`).
- Verified with the isolated `dotnet build` (0 warnings/errors), 103 passing tests, and a real-app check: in an LF file, Ctrl+End, text, Enter, text, and Ctrl+S saved with 0 CR bytes.

2026-09-27: Source-level table editing was added.
- `Shell/MarkdownTableEditor` and `TableRegion` re-align pipe tables (Hangul/CJK counted as double width), move between cells, add/delete rows and columns, and set column alignment.
- Only the table's own text range is replaced, through the editor selection, so outside text and line endings are untouched and each edit is one undo step.
- In Markdown mode, Tab and Shift+Tab move between cells inside a table (Tab on the last cell adds a row). Outside a table, Tab still inserts a tab.
- The right-click menu keeps Cut/Copy/Paste/Select All and shows a Table submenu only when the caret is in a table. Right-click moves the caret to the clicked cell.
- Verified with the isolated `dotnet build` (0 warnings/errors), 98 passing tests, and a real-app check: UI Automation focused the editor, then Ctrl+Home, Tab, X, and Ctrl+S saved `| a   | bbX  |` with CRLF kept.

2026-09-27: Preview inline rendering, accessibility names, and external-change detection were added.
- `Shell/MarkdownInlineParser` renders the syntax the toolbar inserts (bold, italic, strike, link) plus inline code. Links are underlined with the URL in a tooltip and are never navigated. Missing/remote image labels are localized.
- Toolbar glyph buttons, window buttons, and tab close buttons now have localized `AutomationProperties.Name` values and tooltips. Checked with UI Automation: the toolbar reads 제목, 굵게, 기울임, and so on.
- View > Status bar now really hides and shows the status bar.
- `Shell/FileStamp` checks the active file when the window is activated. A clean document reloads silently, a dirty one asks, and a deleted or moved file keeps the text as unsaved with a warning.
- Verified with the isolated `dotnet build` (0 warnings/errors), 89 passing tests, and a launch check: the file was changed on disk, the window was minimized and restored, and the app logged `External change reloaded`.

2026-09-27: View preferences persist and Notepad shortcuts were added.
- `settings.json` now also stores theme, font family, font size, zoom, and word wrap. Older files get defaults, and invalid values are clamped or reset.
- View preferences are saved whenever settings are saved and always on close.
- New shortcuts: Ctrl+wheel zoom, Ctrl+H (replace), Ctrl+W (close tab), and Ctrl+Tab / Ctrl+Shift+Tab (switch tabs).
- Verified with the isolated `dotnet build` (0 warnings/errors), 77 passing tests, and a launch round-trip: the app loaded Dark / Cascadia Mono / 18 / 130% / no-wrap and wrote the same values back on a graceful close. The user's settings were restored afterward.

2026-09-27: Capture-exclusion status is now verified and explained.
- After the set call, `CaptureExclusion.Apply` reads the affinity back with `GetWindowDisplayAffinity`, and the pure `Classify` function derives the status from that value.
- The tooltip spells out what is covered and what is not (open menus/dialogs, phone cameras, other devices, same-user programs). The "limited" state explains the black-box fallback, and Win32 error 8 is shown as "not supported" instead of a memory error.
- `docs/CAPTURE_MATRIX.md` gained a status-meaning table plus rows for menus/dialogs, Recall, RDP, and Teams/Zoom.
- Verified with the isolated `dotnet build` (0 warnings/errors), 74 passing tests, and a launch smoke with privacy requested that logged `status=PrivacyStatusExcluded` from the read-back affinity. The user's `settings.json` was restored afterward.
- The manual tool rows are still not run.

2026-09-27: Encoding preservation and a real word-wrap toggle are complete.
- Files are read as bytes and detected in order: BOM (UTF-8/UTF-16 LE/BE), then strict UTF-8, then the system ANSI code page (for example CP949).
- Saving reuses the detected encoding and BOM. If the text no longer fits the ANSI code page, the user is asked before switching to UTF-8, so characters are never dropped silently.
- The status bar shows the real encoding.
- View > Word wrap now toggles wrapping; it defaults to on, like Notepad.
- The status bar caret position and go-to-line use logical lines via `Shell/TextLines`, so wrapping does not change line numbers.
- Verified with `dotnet build .\Memojang.sln --artifacts-path .\artifacts\build-verify -p:UseAppHost=false` (0 warnings/errors), `dotnet test` (68 passing, including CP949/UTF-8 BOM/UTF-16 byte-for-byte round-trips), and a launch smoke that opened a CP949 sample and logged `encoding=ANSI (949)` with the correct length.
- The default Debug output was locked by a running user instance, so the build used the isolated artifacts path.

2026-07-10: Recent files and capture-exclusion preference now persist across app restarts in `%LOCALAPPDATA%\Memojang\settings.json`. The settings loader keeps up to eight ordered, case-insensitive unique recent paths, safely falls back when the file is missing or invalid, and restores the capture-exclusion request before the window handle applies the native status. The capture controls now represent the saved user request while the status bar continues to report the actual Windows protection result. Verified with focused settings tests, `dotnet build .\Memojang.sln --artifacts-path .\artifacts\build-settings-persistence -p:UseAppHost=false` with zero warnings/errors, and `dotnet test .\Memojang.sln --no-restore --artifacts-path .\artifacts\build-settings-persistence -p:UseAppHost=false` with 54 passing tests.

2026-07-09: File/Edit/View menu expansion is complete. File now exposes recent files, save all, new tab, new txt, and new Markdown flows backed by a minimal multi-tab document state; Edit now exposes visible find/previous find, select all, go-to-line, and font/font-size settings; View now exposes zoom in/out/reset while the status zoom slider remains synchronized. Verified with `dotnet build .\Memojang.sln`, `dotnet test .\Memojang.sln` with 51 passing tests, and a short `Memojang.App.exe` launch smoke. The first build attempt was blocked by an already-running `Memojang.App (18040)` locking the Debug exe; that process was stopped and the clean build passed.

2026-07-09: Main UI quick view controls are complete. The top toolbar now exposes `A-`/`A`/`A+` editor text-size quick buttons plus a dark-mode toggle synchronized with the View theme menu, and the bottom status bar now has a 50-200% zoom slider with a percent readout that scales the editor and Markdown preview view without changing document contents. Verified with `dotnet build .\src\Memojang.App\Memojang.App.csproj -o %TEMP%\memojang-codex-build`, `dotnet test .\tests\Memojang.Tests\Memojang.Tests.csproj -o %TEMP%\memojang-codex-test` with 48 passing tests, and `git diff --check`. A normal Debug output build was blocked by an already-running `Memojang.App (15028)` locking `bin\Debug\net10.0-windows`.

2026-07-09: Custom menu popup contrast/theme follow-up is complete. The app now applies the selected theme before constructing `MainWindow`, supplies explicit menu brush resources for light/dark themes, and uses a Memojang-owned `MenuItem` template so popup borders/backgrounds no longer fall back to unreadable Windows default colors. The tab title also uses the packaged Memojang icon asset. Verified with `dotnet build .\Memojang.sln --artifacts-path artifacts\build-menu-template2 -p:UseAppHost=false`, `dotnet test .\Memojang.sln --artifacts-path artifacts\test-menu-theme -p:UseAppHost=false` with 48 passing tests, `git diff --check`, and an isolated launch smoke from `artifacts\build-menu-template2\bin\Memojang.App\debug\Memojang.App.dll`.

2026-07-08: Review follow-up applied for save/image attachment hardening, Text/Markdown mode UI gating, and custom chrome basics. Save failures now surface localized errors instead of escaping to the UI thread, image attachment and clipboard image write failures show localized errors, unsaved image insertion now forces Markdown Save As defaults, Text mode hides/disables Markdown-specific toolbar/menu actions, and the custom title bar uses system window commands with Alt+Space/right-click system menu access plus accessible window-button names. Verified with `dotnet build .\Memojang.sln -c Release`, `dotnet test .\Memojang.sln -c Release --no-build`, and `git diff --check`. Privacy manual capture matrix checks remain pending.

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
- 2026-07-07: File association opening fixed: startup file-path args now load the target document, menu open shares the same load path, save-as defaults `.txt`/`.md` by document mode, and focused tests pass with `dotnet test .\tests\Memojang.Tests\Memojang.Tests.csproj -p:OutDir=C:\ai\projects\memojang\artifacts\test-bin\`. Full-solution Debug output was locked by running `Memojang.App` processes during verification.
- 2026-07-07: `.txt` default-app launch was verified end-to-end after fixing the real startup failure: `MainWindow.xaml` referenced `Assets/Memojang.ico`, but the icon was not included as a WPF `Resource`. `Memojang.ico` is now included in the project, startup document loading runs after the window is shown, diagnostics write to `%LOCALAPPDATA%\Memojang\startup.log`, and Windows shell-open of a `.txt` file succeeds. Verified with `dotnet build .\Memojang.sln`, focused tests, and a shell-open smoke run.

## Next

- Run manual Windows capture matrix checks for privacy mode and record observed tool behavior.
- Manually check saved `.md` drag/drop image insertion and `.txt` image blocking in a visible desktop session.

## Blockers

- No active blockers.
