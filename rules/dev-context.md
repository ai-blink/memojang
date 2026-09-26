# Dev Context

Date: 2026-09-27

Branch: `main`

Current state:
- Remote `origin` points to `https://github.com/ai-blink/memojang.git`.
- Latest committed implementation baseline is `ad29ac6 feat(settings): persist recent files and capture-exclusion request`.
- Feature/UX review plus similar-editor comparison is saved at `notes/research/2026-09-27-editor-parity-ux-review.md`.
- Program plan exists at `notes/plans/2026-07-06-memojang-program-plan.md`.
- `.NET 10 WPF` solution exists with `Memojang.sln`, `src/Memojang.App`, and `tests/Memojang.Tests`.
- Main window now has a minimal Notepad-like shell: top menu placeholders, central text editor, and status bar placeholders for caret, change state, encoding, and line endings.
- Basic file commands are wired for new/open/save/save-as/exit, with dirty-state reset after open/save and line-ending status detection.
- Dirty documents now prompt before new/open/exit discard, and the app has a process-start smoke check.
- Markdown preview foundation is wired behind a View menu toggle, with lightweight heading, paragraph, and pipe-table rendering.
- Markdown table helper commands can insert a starter table and add a source-preserving empty row to the current table.
- Privacy menu can request window capture exclusion and reports visible/excluded/limited/unavailable status in the status bar, with Win32 detail in the tooltip when available.
- V1 preview folder publish is configured with `PreviewFolder.pubxml` and documented in `docs/PREVIEW_BUILD.md`.
- UI localization/theme foundation is present. Startup selects a supported culture from Windows UI culture with English fallback, applies RTL for Arabic, and uses light/dark resource dictionaries.
- The shell now uses custom WPF chrome: a tab-like title row, compact localized menu row, lightweight Markdown/editor toolbar, custom minimize/maximize/close controls, and drag-to-move handling.
- Harness-launched WPF sessions may miss `WINDIR`/`WinDir`; `App` now fills them from `SystemRoot`/Windows special folder before WPF font cache initialization.
- Document/image V1 slice is implemented: `.txt`/`.md` mode detection, status bar mode text, 200ms Markdown preview debounce, Markdown image parsing/preview, missing-image placeholder, sidecar assets folder copy/link insertion, and `.txt` image attachment blocking.
- Markdown preview polish is implemented in the WPF `FlowDocument` renderer: themed dark/light preview text, `***` separators, fenced code boxes, `md`/`markdown` fenced boxes with nested Markdown/table rendering, empty-heading edge handling, bold emphasis, highlighted `skillhooks`, and unordered list markers (`▶`, `●`, `○`, `▪`) without changing saved source text.
- Toolbar/shell polish is implemented: flatter tab/menu borders, Korean capture-exclusion wording, a left/right capture-exclusion toggle, themed scrollbars, and basic Markdown edit buttons for heading/list/bold/italic/strike/link/table/image/preview.
- External editor research is saved at `notes/research/document-editor-preview-image-patterns-2026-07-07.md`.
- Implementation plan is saved at `notes/plans/2026-07-07-document-preview-image-plan.md`.

Immediate next step (priority from the 2026-09-27 review):
- P0: Detect and preserve file encoding/BOM (currently read via `File.ReadAllText`, always saved as UTF-8 without BOM; CP949 files can be corrupted) and show the real encoding in the status bar.
- P0: Make the Word wrap menu real (it shows checked/disabled while the editor is `TextWrapping="NoWrap"`).
- P0: Capture-exclusion honesty: re-check with `GetWindowDisplayAffinity`, explain the `WDA_MONITOR` "limited" black-box fallback, and add menu popups, Recall, RDP, and OBS rows to the manual capture matrix.
- P1: Persist theme/font/zoom/wrap; Ctrl+wheel zoom, Ctrl+H/W/Tab; external file-change check on activation; session restore (hot exit).
- P2: Preview italic/strike/link, localize hardcoded preview labels, table Tab navigation and row/column context menu, decide translation scope (11 locales at ~15%), toolbar `AutomationProperties.Name`.
- Still open: manual `.md` image drag/drop/menu check and `.txt` image blocking in a visible desktop session.

Known blockers:
- No active blockers.
