# Dev Context

Date: 2026-07-06

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
- UI text now uses resource-backed strings with startup culture selection for Korean, English, Japanese, Hindi, Chinese, French, Arabic, Spanish, Bengali, Portuguese, Russian, and Indonesian. Unsupported Windows UI languages fall back to English, and Arabic applies right-to-left flow.
- Theme foundation now has system/light/dark resource dictionaries and a compact View > App theme selector; startup follows the Windows app theme by default.

Immediate next step:
- Run manual Windows capture matrix checks for privacy mode and record observed tool behavior.
- After the privacy matrix, add a small persistent settings slice for theme/language override if needed.

Known blockers:
- No active blockers.
