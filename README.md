# Memojang

Memojang is a Windows 11-first text and Markdown editor built with .NET 10 WPF. It keeps the familiar, lightweight feel of Notepad while adding focused Markdown/table preview tools and optional window capture exclusion.

## Current Status

Memojang is in active V1 preview development. The current app already includes:

- Notepad-like new/open/save/save-as flows with dirty-document prompts.
- Line, column, encoding, line-ending, document-mode, and capture status indicators.
- Plain text and Markdown document mode detection for `.txt`, `.md`, and `.markdown`.
- Markdown preview with headings, paragraphs, tables, images, separators, fenced code boxes, emphasis, empty-heading handling, and preview-only unordered-list markers.
- Markdown image insertion for saved Markdown documents using a document-local sidecar assets folder.
- Markdown helper toolbar actions for headings, lists, bold, italic, strike, links, tables, images, and preview.
- Light/dark theme resources, localized UI text foundation, and Arabic right-to-left flow support.
- Optional best-effort Windows capture exclusion with honest visible/excluded/limited/unavailable status.

Memojang is not a full word processor, spreadsheet editor, or DRM system. Capture exclusion is an operating-system feature with known limits, and the app reports the actual protection status instead of promising absolute secrecy.

## Requirements

- Windows 11
- .NET 10 SDK for development
- Matching .NET desktop runtime for framework-dependent preview builds

## Build And Test

```powershell
dotnet --info
dotnet build .\Memojang.sln
dotnet test .\Memojang.sln
```

Run the debug app:

```powershell
Start-Process .\src\Memojang.App\bin\Debug\net10.0-windows\Memojang.App.exe
```

Create a preview folder publish:

```powershell
dotnet publish .\src\Memojang.App\Memojang.App.csproj -p:PublishProfile=PreviewFolder
```

Published output:

```text
artifacts\publish\Memojang-preview\Memojang.App.exe
```

## Project Map

- `src/Memojang.App/` - WPF application source.
- `tests/Memojang.Tests/` - automated parser, document-mode, image, table, and shell helper tests.
- `doc/INTENT.md` - product target, acceptance shape, and non-goals.
- `docs/PREVIEW_BUILD.md` - preview publish command and output path.
- `rules/dev-context.md` - current implementation snapshot.
- `rules/dev-progress.md` - live progress and blockers.
- `rules/dev-roadmap.md` - milestone roadmap.
- `rules/dev-decisions.md` - durable decisions.
- `notes/plans/` and `notes/research/` - planning and research artifacts.

## Development Notes

`CLAUDE.md` is the canonical project instruction entrypoint. Keep source changes scoped, preserve the Notepad-like Windows UX target, and keep Markdown preview polish as renderer behavior unless a feature explicitly changes saved source text.
