using System.Runtime.InteropServices;

namespace Memojang.App.WindowsInterop;

public sealed record CaptureExclusionResult(bool IsEnabled, bool IsProtected, string StatusText);

public static class CaptureExclusion
{
    private const uint WdaNone = 0x00000000;
    private const uint WdaMonitor = 0x00000001;
    private const uint WdaExcludeFromCapture = 0x00000011;

    public static CaptureExclusionResult Apply(IntPtr windowHandle, bool enabled)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return new CaptureExclusionResult(enabled, false, "Capture status unavailable");
        }

        if (!enabled)
        {
            return SetWindowDisplayAffinity(windowHandle, WdaNone)
                ? new CaptureExclusionResult(false, false, "Capture visible")
                : new CaptureExclusionResult(false, false, "Capture reset failed");
        }

        if (SetWindowDisplayAffinity(windowHandle, WdaExcludeFromCapture))
        {
            return new CaptureExclusionResult(true, true, "Capture excluded");
        }

        if (SetWindowDisplayAffinity(windowHandle, WdaMonitor))
        {
            return new CaptureExclusionResult(true, true, "Capture limited");
        }

        return new CaptureExclusionResult(true, false, "Capture exclusion unavailable");
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
}
