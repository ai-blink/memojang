# Document Editor Preview And Image Patterns

Date: 2026-07-07

## Summary

Memojang V1 should keep `.txt` as plain text and make `.md` the only image-capable document mode. The strongest reusable pattern across local-first Markdown editors is Markdown source as truth plus sidecar/local asset files and relative links. Full WYSIWYG, block databases, server uploads, and proprietary packages are useful references, but they are too heavy for the current Notepad-like target.

Recommended V1 direction:
- `.txt`: plain text only; no hidden Markdown image syntax insertion.
- `.md` / `.markdown`: source editing, debounced preview, local image preview, image files copied into `<document-name>.assets/`.
- Custom extension: document as V2 design topic only; do not implement in V1.
- Preview: keep current FlowDocument path and add local images/placeholders before considering WebView2.

## External Editor Matrix

| Editor | Editing Model | Preview / Live Editing | Image Attachment | Asset Storage | Format Strategy | Memojang Lesson |
|---|---|---|---|---|---|---|
| Obsidian | Markdown files in a local vault | Live Preview plus Reading view | Drag/drop embeds supported for vault files | Vault-local files; configurable attachment location | Plain Markdown plus `.obsidian` settings | Keep user-owned files and relative/local links |
| Typora | Markdown source with live preview editing | Inline live preview | Drag/drop, menu insert, clipboard images | Original path by default; can copy to configured folder | Plain `.md`; optional front matter hints | Copy image into a predictable folder and insert a relative link |
| MarkText | WYSIWYG-ish realtime Markdown editor | In-place realtime preview | Clipboard image paste advertised | Local file-oriented handling | Plain Markdown | Avoid absolute path defaults; keep generated links portable |
| Zettlr | Markdown workspace | Preview/raw rendering options | Local image paths and default image path setting | Workspace files, no hidden package by default | Plain Markdown plus Pandoc/export features | Local file workflow fits Memojang better than a database |
| VS Code | Source editor plus preview pane | Side-by-side preview, realtime updates, sync | Drag/drop and paste can insert Markdown links and copy media into workspace | Workspace-relative files | Plain `.md` | Side-by-side source + preview is transparent and Git-friendly |
| Notion | Cloud/block editor | Blocks render in place | Upload/embed blocks, drag/drop | Cloud workspace storage | Object/block database | Good UX reference, poor fit for local plain-file V1 |
| Logseq | Outliner/block graph over Markdown/Org; DB mode emerging | Inline block rendering | Assets folder pattern in graph workflows | `assets/`, `pages/`, `journals/` style graph folders | Markdown/Org graph or DB mode | Plain-file mode is portable; DB mode is a V2+ tradeoff |
| Joplin | Markdown notes plus rich-text editor | Split/viewer and WYSIWYG paths | Attach button, drag/drop, clipboard image paste | Managed `resources/` with IDs | SQLite/profile plus export formats | Managed resources are robust but less Git-transparent |
| HedgeDoc | Collaborative server Markdown | editor/view/both modes | Server image upload | Server filesystem/S3/MinIO URLs | Server notes and permissions | Avoid server URL dependency for a local editor |
| Tiptap/ProseMirror | Structured rich editor toolkit | App-defined WYSIWYG/in-place | File handlers expose paste/drop; app must store/upload | App-defined | JSON/HTML/Markdown conversion | Useful future architecture, not a V1 storage default |

## Preview / Live Editing Patterns

Source-plus-preview is the best V1 match. VS Code keeps the Markdown source visible and previews side-by-side with real-time updates. Typora, MarkText, Zettlr, and Obsidian show that inline/live preview is desirable, but implementing a full rich editor would widen Memojang beyond the current lightweight WPF slice.

For Memojang V1:
- Keep source editing as the saved truth.
- Keep preview optional and side-by-side.
- Debounce preview updates at 150-300ms to avoid rendering on every keystroke.
- Treat scroll sync, rich inline editing, and full Markdown coverage as follow-up.

## Image Attachment Patterns

The safest local-first image flow is:
1. User drops/pastes/selects an image while editing a saved Markdown file.
2. App copies the original image into a document-local assets folder.
3. App inserts a relative Markdown image link.
4. Preview resolves the relative path from the current document directory.

Typora and VS Code support variants of this local-copy/link pattern. Joplin proves managed resources are practical but less transparent. Notion and HedgeDoc show why upload/server models are poor fits for Memojang V1.

For unsaved Markdown documents, V1 should require saving first. Temporary image storage would require orphan cleanup, Save As migration, and crash recovery rules, which is not worth the V1 complexity.

## Asset Storage Patterns

Recommended V1 sidecar:

```text
notes/example.md
notes/example.assets/image-20260707-120000.png
```

Inserted Markdown:

```markdown
![source](example.assets/image-20260707-120000.png)
```

Rules:
- Copy original image; never move it.
- Generate collision-free filenames with suffixes.
- Store links relative to the Markdown document.
- Keep remote URL fetching out of V1 preview by default.

## Custom Format / Package Format Patterns

Candidate custom extensions:
- `.mjd`: short and distinctive, but not self-explanatory.
- `.memojang`: clear branding, too long for frequent document names.
- `.mmdoc`: document-like, but easy to confuse with Markdown variants.

Package options:
- Zip package: `document.md` + `assets/`; portable but opaque to Git diffs unless unpacked.
- Folder package: `document.mjd/`; transparent but awkward on Windows for users expecting a file.
- Markdown + sidecar assets: most transparent and V1-friendly.
- JSON metadata + Markdown body + assets: useful later for tabs, privacy settings, or document metadata, but heavier than V1 needs.

Conclusion: keep Markdown + sidecar assets for V1. Revisit a package/custom extension only after Markdown image/table workflows are stable.

## Lessons For Memojang V1

- Use `.txt` and `.md` as explicit document modes.
- Show document mode in the status bar.
- Disable image/table/preview commands when the current document is plain text.
- Render local Markdown images in FlowDocument preview with width limits.
- Show a placeholder for missing/remote/unsupported images instead of crashing.
- Prefer sidecar assets over hidden app databases.
- Keep toolbar controls honest: implemented commands enabled, future commands disabled.

## What To Avoid

- Do not silently insert Markdown image syntax into `.txt`.
- Do not default to absolute paths for dropped images.
- Do not introduce WebView2, ProseMirror, or block editing as V1 infrastructure.
- Do not implement a custom extension before the plain Markdown workflow is proven.
- Do not make uploaded/server URLs the default asset strategy.

## Sources

- Obsidian accepted formats and embeds: https://obsidianmd-obsidian-help.mintlify.app/files/accepted-formats
- Obsidian data storage: https://obsidianmd-obsidian-help.mintlify.app/files/how-obsidian-stores-data
- Obsidian embed files: https://obsidianmd-obsidian-help.mintlify.app/linking/embed-files
- Typora images: https://support.typora.io/Images/
- VS Code Markdown: https://code.visualstudio.com/docs/languages/markdown
- MarkText README: https://github.com/marktext/marktext
- Zettlr settings: https://docs.zettlr.com/en/reference/settings/
- Zettlr Markdown basics: https://docs.zettlr.com/de/reference/markdown-basics/
- Joplin attachments: https://joplinapp.org/help/apps/attachments/
- Notion images/files/media: https://www.notion.com/help/images-files-and-media
- Anytype storage: https://doc.anytype.io/anytype-docs/advanced/data-and-security/data-storage-and-deletion
- Logseq existing Markdown graph discussion: https://discuss.logseq.com/t/how-to-create-a-logseq-graph-using-existing-markdown-files/8431
- HedgeDoc configuration uploads: https://docs.hedgedoc.org/configuration/
- Tiptap image extension: https://tiptap.dev/docs/editor/extensions/nodes/image
- Tiptap FileHandler extension: https://tiptap.dev/docs/editor/extensions/functionality/filehandler
- ProseMirror Markdown example: https://prosemirror.net/examples/markdown/
- ProseMirror upload example: https://prosemirror.net/examples/upload/
