# Dev Architecture

Target stack: .NET 10 WPF, `net10.0-windows`.

Planned layers:
- `src/Memojang.App/` - WPF app entry, shell, tabs, menus, settings, and composition.
- `src/Memojang.App/Editor/` - text editing adapter, document state, caret/status behavior.
- `src/Memojang.App/Markdown/` - Markdig pipeline, preview rendering, and safe HTML policy.
- `src/Memojang.App/Markdown/Tables/` - Markdown table parsing, editing commands, and source-preserving updates.
- `src/Memojang.App/WindowsInterop/` - HWND, `SetWindowDisplayAffinity`, DWM and Windows-specific helpers.
- `tests/Memojang.Tests/` - parser, document model, table editing, and interop status tests.
- `samples/markdown/` - Markdown fixtures for table-heavy and encoding/manual checks.

Dependency rules:
- Keep Windows interop behind a narrow interface so UI code can report status without owning P/Invoke details.
- Keep Markdown source as the saved truth; preview/table helpers must round-trip to Markdown.
- If WebView2 is used, serve local controlled HTML only and block remote network/script surfaces.
- Do not copy Notepad or reference clone assets; implement Memojang-owned UI/resources.

Test strategy:
- Unit test Markdown/table transforms and encoding decisions.
- Smoke test app startup and basic file operations once UI automation exists.
- Manual verification remains required for capture exclusion and Windows shell behavior.

Diagram index: [docs/diagrams/README.md](../docs/diagrams/README.md)
