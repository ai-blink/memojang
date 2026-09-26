# Dev Decisions

## Vision

- D-001: Memojang targets Windows 11 Notepad-like speed and familiarity, not a heavy IDE or word processor.
- D-002: Markdown/table support is a focused extension of Notepad-like editing, not a full document suite.

## Layer Boundaries

- D-003: Markdown source remains the saved truth; previews and table helpers must round-trip to source.
- D-004: Windows capture exclusion lives behind `WindowsInterop` and reports status to UI instead of being treated as guaranteed DRM.

## Executor Policy

- D-005: `CLAUDE.md` is the only canonical project instruction entrypoint; do not add authoritative `AGENTS.md`.
- D-006: Non-trivial work must define finish line, acceptance checks, scope limit, review budget, and stop rule.

## Security

- D-007: Capture protection must be described as best-effort OS capture exclusion with known limits, not absolute content protection.
- D-008: WebView2 preview, if used, must use local controlled content and block remote surfaces by default.

## Workflow

- D-009: Live progress belongs in `rules/dev-progress.md`; milestone roadmap belongs in `rules/dev-roadmap.md`.

## Infra

- D-010: Target runtime is .NET 10 LTS with WPF unless a later explicit plan changes the stack.

## Documents

- D-011: V1 keeps `.txt` as plain text and enables image attachment only for `.md`/`.markdown` documents.
- D-012: V1 Markdown images use a document-local sidecar assets folder with relative links; custom Memojang package extensions remain V2 candidates.
- D-013: Markdown source remains unchanged for preview-only polish; list markers, fenced Markdown box rendering, emphasis styling, and empty-heading handling are renderer behavior only.
- D-014: Files are saved in the encoding they were opened with. Detection order is BOM, then strict UTF-8, then the system ANSI code page; new documents are UTF-8 without BOM. Switching an ANSI document to UTF-8 because of non-representable characters requires user consent.
- D-016: Session restore (hot exit) is on by default, like Windows 11 Notepad. Closing saves tabs and unsaved text to the app's session file instead of prompting, never auto-saves user files, and falls back to prompts if the session write fails.
- D-015: Word wrap defaults to on like Notepad. Status bar and go-to-line always use logical file lines, not WPF visual lines.
