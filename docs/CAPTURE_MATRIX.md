# Capture Exclusion Matrix

Privacy mode asks Windows for a display affinity. After asking, it reads the value back with `GetWindowDisplayAffinity`, so the status bar shows what Windows actually applied rather than what the request claimed.

This is best-effort OS capture exclusion, not DRM. Hover the privacy status to see what is covered and any Win32 detail.

## Status Meaning

| Status | Affinity read back | Meaning |
|---|---|---|
| Capture excluded | `WDA_EXCLUDEFROMCAPTURE` (0x11) | The window is omitted from most screenshots and recordings. |
| Capture limited | `WDA_MONITOR` (0x1) | Older Windows mode: captures show the window as a black box. |
| Capture exclusion unavailable | `WDA_NONE` | The request failed. Win32 error 8 means the window or driver is not supported, not a memory problem. |
| Capture status unavailable | unreadable | No window handle yet, or the read-back failed. |

## Checks

| Tool | Privacy off expected | Privacy on expected | Result | Notes |
|---|---|---|---|---|
| Snipping Tool | Window content visible | Window omitted | Not run | Manual check required |
| Print Screen | Window content visible | Window omitted | Not run | Manual check required |
| OBS Display Capture | Window content visible | Window omitted (WGC) | Not run | Try both capture methods |
| OBS Window Capture | Window content visible | Window omitted or blank | Not run | Manual check required |
| Teams/Zoom screen share | Window content visible | Window omitted | Not run | Manual check required |
| Open menu or dialog while privacy on | Visible | **Expected visible**: popups and dialogs are separate windows without affinity | Not run | Documents the known gap |
| Recall or screenshot history | Visible where available | Unverified: Microsoft docs cite `WDA_MONITOR` for Recall | Not run | Requires a Copilot+ PC |
| RDP session (viewer side) | Visible | Unverified | Not run | Manual check required |
| Phone camera or other device | Visible | Visible, not preventable | N/A | Stated in the status tooltip |

2026-09-27 automated read-back on the dev machine: with privacy requested at startup, the app logged `status=PrivacyStatusExcluded` (affinity 0x11). This proves the setting was applied. It does not replace the tool rows above.

## Pass Rule

- Privacy off must leave the window visible to normal capture.
- Privacy on must show a status derived from the read-back affinity, never from the set call alone.
- Failure states expose Win32 detail in the status tooltip. Error 8 is shown as "not supported".
- Unsupported capture paths are acceptable only when the app reports the limitation instead of promising absolute protection.
