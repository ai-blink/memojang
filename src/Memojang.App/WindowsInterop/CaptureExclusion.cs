using System.Runtime.InteropServices;
using Memojang.App.Localization;

namespace Memojang.App.WindowsInterop;

public sealed record CaptureExclusionResult(
    bool IsEnabled,
    bool IsProtected,
    string StatusKey,
    int? ErrorCode = null)
{
    // ERROR_NOT_ENOUGH_MEMORY is what Windows returns for unsupported windows (layered/transparent) or drivers,
    // so showing its literal text would mislead the user.
    private const int ErrorNotEnoughMemory = 8;

    public string StatusText => UiText.Get(StatusKey);

    public string DetailText
    {
        get
        {
            string detail = StatusKey switch
            {
                "PrivacyStatusExcluded" => UiText.Get("PrivacyDetailExcluded"),
                "PrivacyStatusLimited" => UiText.Get("PrivacyDetailLimited"),
                _ => StatusText
            };

            return ErrorCode switch
            {
                null => detail,
                ErrorNotEnoughMemory => $"{detail}\n{UiText.Get("PrivacyErrorNotSupported")} (Win32 {ErrorCode})",
                _ => $"{detail} (Win32 {ErrorCode})"
            };
        }
    }
}

public static class CaptureExclusion
{
    public const uint WdaNone = 0x00000000;
    public const uint WdaMonitor = 0x00000001;
    public const uint WdaExcludeFromCapture = 0x00000011;

    public static CaptureExclusionResult Apply(IntPtr windowHandle, bool enabled)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return new CaptureExclusionResult(enabled, false, "PrivacyStatusUnavailable");
        }

        int? setError = null;
        if (!enabled)
        {
            setError = SetAffinity(windowHandle, WdaNone);
        }
        else if (SetAffinity(windowHandle, WdaExcludeFromCapture) is int excludeError)
        {
            // Pre-2004 Windows only supports WDA_MONITOR, which captures the window as a black box.
            setError = SetAffinity(windowHandle, WdaMonitor) is null ? null : excludeError;
        }

        // The status reflects what Windows reports back, not what the set call claimed.
        uint? actual = GetWindowDisplayAffinity(windowHandle, out uint affinity) ? affinity : null;
        return Classify(enabled, actual, setError);
    }

    public static CaptureExclusionResult Classify(bool requested, uint? actualAffinity, int? setError)
    {
        return actualAffinity switch
        {
            null => new CaptureExclusionResult(requested, false, "PrivacyStatusUnavailable", setError),
            WdaExcludeFromCapture => new CaptureExclusionResult(requested, true, "PrivacyStatusExcluded"),
            WdaMonitor => new CaptureExclusionResult(requested, true, "PrivacyStatusLimited"),
            _ when requested => new CaptureExclusionResult(true, false, "PrivacyStatusExclusionUnavailable", setError),
            WdaNone => new CaptureExclusionResult(false, false, "PrivacyStatusVisible"),
            _ => new CaptureExclusionResult(false, false, "PrivacyStatusResetFailed", setError)
        };
    }

    private static int? SetAffinity(IntPtr windowHandle, uint affinity) =>
        SetWindowDisplayAffinity(windowHandle, affinity) ? null : Marshal.GetLastWin32Error();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowDisplayAffinity(IntPtr hWnd, out uint pdwAffinity);
}
