# Dev Context

Date: 2026-07-07

Branch: `main`

Current state:
- Remote `origin` points to `https://github.com/ai-blink/memojang.git`.
- Latest known committed implementation baseline is `8e3ab8e feat(project): scaffold Memojang WPF app`.
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

Immediate next step:
- Run manual Windows capture matrix checks for privacy mode and record observed tool behavior.
- Manually check saved `.md` image drag/drop/menu insertion and `.txt` image blocking in a visible desktop session.

Known blockers:
- No active blockers.
