# Capture Exclusion Matrix

Memojang privacy mode uses Windows display affinity and reports the result in the status bar. This is a best-effort OS request, not DRM. Hover the privacy status text to see any Win32 error detail when exclusion is unavailable or reset fails.

## Checks

| Tool | Privacy off expected | Privacy on expected | Result | Notes |
|---|---|---|---|---|
| Snipping Tool | Window content visible | Window content hidden, blank, or excluded | Not run | Manual check required |
| Print Screen | Window content visible | Window content hidden, blank, or excluded | Not run | Manual check required |
| OBS Display Capture | Window content visible | Behavior depends on capture mode and OS support | Not run | Manual check required |
| OBS Window Capture | Window content visible | Behavior depends on capture mode and OS support | Not run | Manual check required |
| Recall or screenshot history | Window content visible where available | Behavior depends on Windows feature availability | Not run | Manual check required |

## Pass Rule

- Privacy off must leave the window visible to normal capture.
- Privacy on must show an honest in-app status: `Capture excluded`, `Capture limited`, or `Capture exclusion unavailable`.
- Failure states should expose Win32 detail in the status tooltip when available.
- Unsupported capture paths are acceptable only when the app reports limitation or unavailability instead of promising absolute protection.
