using Memojang.App.Shell;
using Memojang.App.Theme;

namespace Memojang.App;

public partial class MainWindow
{
    private readonly AppSettingsStore _settingsStore = new();

    private void InitializePersistentSettings()
    {
        AppSettings settings = _settingsStore.Load();

        _recentFilePaths.Clear();
        _recentFilePaths.AddRange(settings.RecentFiles);

        _captureExclusionRequested = settings.CaptureExclusionEnabled;
        CaptureExclusionMenuItem.IsChecked = _captureExclusionRequested;
        ToolbarPrivacyButton.IsChecked = _captureExclusionRequested;

        SetEditorFontFamily(settings.EditorFontFamily);
        SetEditorFontSize(settings.EditorFontSize);
        SetZoom(settings.ZoomPercent);
        SetWordWrap(settings.WordWrap);
        _sessionRestoreEnabled = settings.SessionRestore;
        SessionRestoreMenuItem.IsChecked = _sessionRestoreEnabled;
        if (Enum.TryParse(settings.Theme, out AppTheme theme) && theme != AppTheme.System)
        {
            ApplyTheme(theme);
        }
    }

    // Recent files and capture changes save immediately; view preferences are captured whenever any save
    // happens and always on close, so dragging the zoom slider does not rewrite the file per tick.
    private void SavePersistentSettings()
    {
        _settingsStore.Save(new AppSettings
        {
            RecentFiles = [.. _recentFilePaths],
            CaptureExclusionEnabled = _captureExclusionRequested,
            Theme = AppThemeService.RequestedTheme.ToString(),
            EditorFontFamily = _editorFontFamilyName,
            EditorFontSize = _editorBaseFontSize,
            ZoomPercent = _zoomPercent,
            WordWrap = WordWrapMenuItem.IsChecked,
            SessionRestore = _sessionRestoreEnabled
        });
    }
}
