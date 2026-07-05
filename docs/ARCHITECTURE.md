# Architecture

Memojang is planned as a .NET 10 WPF Windows desktop app.

The authoritative live architecture summary is `rules/dev-arch.md`. This file stays stable and points to deeper artifacts instead of duplicating the live file map.

## Current Shape

- WPF shell owns windows, tabs, menus, settings, status, and view composition.
- Editor layer owns document text state and Notepad-like editing behavior.
- Markdown layer owns parsing, preview rendering, and table helpers.
- Windows interop layer owns HWND capture exclusion and related OS status.

## Diagrams

- [Diagram index](diagrams/README.md)
- [Architecture overview](diagrams/architecture-overview.md)
