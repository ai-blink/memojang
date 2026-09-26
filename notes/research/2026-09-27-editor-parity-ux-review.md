# Editor Parity and UX Review (2026-09-27)

Sources: two subagent passes. The first was a read-only code review of `src/Memojang.App`. The second compared similar repos and products, with GitHub API values captured on 2026-09-27. It extends `document-editor-preview-image-patterns-2026-07-07.md` without repeating it.

Framing: Windows 11 Notepad already ships tabs, session restore, Markdown formatting, and table insert with row/column add/delete. Memojang's gap is Notepad basics and honest status, not differentiation.

## Code Findings (verified file:line at review time)

| Area | Finding | Evidence |
|---|---|---|
| Encoding | Reads with `File.ReadAllText` and always writes UTF-8 without BOM. Status is fixed to "UTF-8". CP949/ANSI and UTF-16 are not detected or preserved. | `MainWindow.Files.cs:21,52,136`, `MainWindow.xaml.cs:365` |
| Line endings | Detected for status only. No save-time normalization is visible, so this needs a test. | `Shell/LineEnding.cs`, `MainWindow.Files.cs:136` |
| Word wrap | The menu is `IsChecked=True IsEnabled=False` while the editor is `TextWrapping="NoWrap"`, so the UI misreports the state. | `MainWindow.xaml:250,485` |
| Status bar toggle | Dead menu item (disabled). | `MainWindow.xaml:251` |
| Redo | No Edit menu item. | `MainWindow.xaml:220` |
| Find | No case-sensitive option (always ignore-case). | `MainWindow.Search.cs:94,117,156` |
| Zoom | No Ctrl+wheel handler; keys and slider only. | `MainWindow.xaml.cs:320-334` |
| Tabs | No drag reorder, Ctrl+W, or Ctrl+Tab. | `MainWindow.Tabs.cs:187-247` |
| Preview | Custom regex parser, not Markdig as `rules/dev-arch.md` states. Only bold renders inline. Italic, strike, and links from the toolbar stay as raw text. No ordered list, quote, or inline code. | `Shell/MarkdownPreviewDocumentFactory.cs:163-192` |
| Preview labels | "Missing image" and "Remote image not loaded" are hardcoded in English. | `MarkdownPreviewDocumentFactory.cs:288-290` |
| Settings | Only recent files and the capture request persist. Theme, font, size, zoom, and wrap reset on restart. | `Shell/AppSettingsStore.cs:7-12` |
| Theme | System dark/light is read once at startup, with no `WM_SETTINGCHANGE` follow. | `Theme/AppThemeService.cs:46-49` |
| Localization | `Strings.resx` and `ko` have 99 keys. The other 11 locales have 15 keys (about 15%), yet the roadmap marks this Done. | `Resources/Strings.*.resx` |
| Accessibility | Toolbar glyph buttons ("H1", "B", and others) lack `AutomationProperties.Name`. | `MainWindow.xaml:293-373` |
| Capture exclusion | `WDA_MONITOR` fallback is labeled "limited" without saying it captures as a black box. There is no `GetWindowDisplayAffinity` re-check. Menu `Popup` is `AllowsTransparency=True` in a separate HWND, so an open menu is likely capturable (inferred, needs measurement). | `WindowsInterop/CaptureExclusion.cs:42-44`, `Styles/Controls.xaml:129` |
| Tests | Only `Shell/*` logic is covered; there are no `MainWindow` flow tests. `samples/` is missing. | `tests/Memojang.Tests/` |

## Similar Products

| Product | Stack / license | Relevant takeaways |
|---|---|---|
| [Notepads](https://github.com/0x7c13/Notepads) | UWP, MIT, 10.2k stars | 7s session backup; status-bar encoding flyout with separate "Reopen with" and "Save with"; external-change icon without a modal; tab drag-out |
| [NotepadNext](https://github.com/dail8859/NotepadNext) | Qt, GPL-3.0 | Restore session plus unsaved changes; search across all docs |
| [Notepad++](https://github.com/notepad-plus-plus/notepad-plus-plus) | Win32, GPL-3 | Periodic snapshot backup; encoding "reinterpret" vs "Convert to" |
| [Notepad3](https://github.com/rizonesoft/Notepad3) | C, BSD-3 | uchardet detection; five file-watch modes; autosave off by default |
| [Win11 Notepad](https://learn.microsoft.com/en-us/windows-insider/release-notes/apps/notepad) | proprietary | Session restore; Mica; single-surface formatted Markdown with a syntax toggle and a setting to disable formatting; table row/column context menu |
| [MarkText](https://github.com/marktext/marktext) | Electron, MIT | Hover table toolbar (align, insert/delete row/column) |
| [Typora](https://support.typora.io/Table-Editing/) | proprietary | `Ctrl+Enter` adds a row; word-count popup; draft recovery |
| [Markdown Monster](https://github.com/RickStrahl/MarkdownMonster) | WPF, source-visible, no redistribution | External change: silent reload when clean, Yours/Theirs/Compare when dirty; grid table editor |
| [Advanced Tables](https://github.com/tgrosinger/advanced-tables-obsidian) / [Zettlr](https://docs.zettlr.com/en/editor/tables/) | MIT / GPL-3.0 | Tab, Shift+Tab, and Enter move between cells and re-align the table |

## Capture Exclusion Limits

- [MS Learn](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity): works only on top-level windows the calling process owns and only while DWM composes. It cannot stop a phone camera.
- Blocked per secondary sources: BitBlt and WGC (OBS, Teams, Zoom, PrintScreen). Unverified: RDP, VM host, Miracast, capture cards.
- Recall: Microsoft's docs cite `WDA_MONITOR`. Whether `WDA_EXCLUDEFROMCAPTURE` works for Recall rests only on indirect evidence (Signal), so it needs measurement.
- Layered (`AllowsTransparency=True`) windows are unsupported and fail with a misleading `0x8`. Do not switch the main window to `AllowsTransparency=True` (for example, to get Mica).
- Same-user injected code can reset the affinity to `WDA_NONE`.
- Display precedents: [Bitwarden](https://bitwarden.com/help/app-settings/) says "most screenshots"; [KeePassXC](https://github.com/keepassxreboot/keepassxc/pull/6030) shows a title marker when capture is allowed; [Signal](https://signal.org/blog/signal-doesnt-recall/) warns before disabling.

## Priority

- **P0**
  - Encoding detection and BOM preservation, with the real encoding in the status bar.
  - A real word-wrap toggle.
  - Capture-exclusion honesty: `GetWindowDisplayAffinity` re-check, a translated `0x8` message, explanation of the black-box fallback, and matrix rows for menus, Recall, RDP, and OBS.
- **P1**
  - Persist theme, font, zoom, and wrap.
  - Ctrl+wheel zoom and Ctrl+H/W/Tab.
  - External-change check on activation.
  - Session restore (hot exit under `%LOCALAPPDATA%`, never autosave the original file).
- **P2**
  - Preview italic, strike, and links; localize preview labels.
  - Table Tab navigation and a row/column context menu.
  - Translation scope decision.
  - Toolbar accessibility names.
- **V2**
  - Tab drag-out.
  - Detached preview window.
  - Focus mode.
  - Unsaved-changes diff.
  - Mica.

## License Boundary

- **Behavior reference only:** GPL (NotepadNext, Notepad++, Zettlr, ghostwriter), AGPL (Signal), Markdown Monster (no redistribution), proprietary products, and repos without a license.
- **Code reuse allowed with the notice kept:** Notepads, MarkText, and Invisiwind (MIT); Notepad3 and Notepad2e (BSD-3).
