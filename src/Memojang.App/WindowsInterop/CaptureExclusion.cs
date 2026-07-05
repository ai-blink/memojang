using System.Runtime.InteropServices;
using Memojang.App.Localization;

namespace Memojang.App.WindowsInterop;

public sealed record CaptureExclusionResult(
    bool IsEnabled,
    bool IsProtected,
    string StatusKey,
    int? ErrorCode = null)
{
    public string StatusText => UiText.Get(StatusKey);

    public string DetailText => ErrorCode is null ? StatusText : $"{StatusText} (Win32 {ErrorCode})";
}

public static class CaptureExclusion
{
    private const uint WdaNone = 0x00000000;
    private const uint WdaMonitor = 0x00000001;
    private const uint WdaExcludeFromCapture = 0x00000011;

    public static CaptureExclusionResult Apply(IntPtr windowHandle, bool enabled)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return new CaptureExclusionResult(enabled, false, "PrivacyStatusUnavailable");
        }

        if (!enabled)
        {
            return SetWindowDisplayAffinity(windowHandle, WdaNone)
                ? new CaptureExclusionResult(false, false, "PrivacyStatusVisible")
                : new CaptureExclusionResult(false, false, "PrivacyStatusResetFailed", Marshal.GetLastWin32Error());
        }

        if (SetWindowDisplayAffinity(windowHandle, WdaExcludeFromCapture))
        {
            return new CaptureExclusionResult(true, true, "PrivacyStatusExcluded");
        }

        if (SetWindowDisplayAffinity(windowHandle, WdaMonitor))
        {
            return new CaptureExclusionResult(true, true, "PrivacyStatusLimited");
        }

        return new CaptureExclusionResult(true, false, "PrivacyStatusExclusionUnavailable", Marshal.GetLastWin32Error());
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
}
