# Document Preview And Image Plan

Date: 2026-07-07

## Finish Line

Design Memojang's V1 document mode and Markdown image policy, then implement the smallest useful V1 slice:
- `.txt` / `.md` document mode detection.
- status bar document mode display.
- Markdown preview debounce.
- Markdown image syntax parsing and local preview with missing-image placeholders.
- saved Markdown image insertion through file dialog, drag/drop, and clipboard image paste.
- sidecar assets folder copy with relative Markdown links.
- plain text image attachment blocking.

## Scope Limit

In scope:
- Current WPF/FlowDocument preview implementation.
- Existing single-window file command model.
- Local image files and clipboard bitmap images.
- `.txt`, `.md`, `.markdown`.

Out of scope:
- WebView2 preview.
- Full WYSIWYG/block editor.
- Remote image fetch.
- Custom document extension implementation.
- Asset garbage collection.
- Preview scroll sync.
- Full Markdown parser replacement.

## Acceptance Checks

- `dotnet build .\Memojang.sln`
- `dotnet test .\Memojang.sln`
- App launch smoke.
- `git diff --check`
- Manual Markdown checks:
  - saved `.md` shows `Markdown` mode and can toggle preview.
  - `![alt](relative.png)` renders if present and shows a placeholder if missing.
  - image drop/menu insert copies into `<document>.assets/` and inserts a relative link.
  - `.txt` shows `Text` mode and blocks image attachment.

## Image Attachment Policy

Saved `.md` / `.markdown`:
- Copy dropped/selected/pasted image data into `<document-name>.assets/`.
- Use `image-YYYYMMDD-HHMMSS.ext`, adding `-2`, `-3`, etc. for collisions.
- Insert a document-relative Markdown image link.
- Keep the original file untouched.

Unsaved document:
- V1 policy is save-first.
- On image attach, ask the user to save; proceed only if the saved file is Markdown.
- Do not create temporary attachment state in V1.

`.txt`:
- Image attachment is blocked.
- Do not insert Markdown image syntax into plain text mode.

## Document Mode Policy

- New unsaved documents start as `Text`.
- Opening or saving as `.md` / `.markdown` switches to `Markdown`.
- Opening or saving as `.txt` or any other extension switches to `Text`.
- Markdown menu, preview toggle, table helper, and image insertion are enabled only in Markdown mode.
- Future unimplemented toolbar controls remain disabled until backed by commands.

## Recommended Direction

V1 should follow a VS Code/Typora-style local Markdown workflow rather than a Notion/Joplin-style object database:
- Source text remains the saved truth.
- Preview is derived and replaceable.
- Assets are ordinary files beside the document.
- Git and backup tools can see both Markdown and assets.

Custom extensions stay V2:
- Prefer documenting `.mjd`, `.memojang`, and `.mmdoc` tradeoffs now.
- Do not add package readers/writers until Markdown image workflows are stable.

## Minimal Implementation Slice

Implemented in this run:
1. Added `DocumentModeDetector`.
2. Added status bar document mode display.
3. Added 200ms preview debounce for text changes.
4. Added image blocks to the Markdown preview parser.
5. Added local image path resolution from the current document.
6. Added FlowDocument image preview and missing/remote placeholders.
7. Added sidecar asset copy/link generation helper.
8. Added menu, drag/drop, and clipboard image insertion paths for Markdown.
9. Disabled unimplemented toolbar formatting controls.
10. Added unit tests for mode detection, image parsing, relative path resolution, sidecar copy, and collision handling.

## Follow-ups

- Add manual sample files under `samples/markdown/` and `samples/assets/`.
- Add a small UI automation or scripted smoke for drag/drop if the harness can support it.
- Add localized strings for all supported cultures instead of relying on neutral fallback for new keys.
- Decide whether remote image URLs should be shown as placeholders permanently or become opt-in preview content.
- Revisit Markdig only after lists/code/links become part of the V1 preview finish line.
- Run and record the privacy capture matrix separately; this image slice does not close that pending roadmap item.
