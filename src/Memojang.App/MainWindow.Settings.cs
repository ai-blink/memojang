using System.Windows;
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
        SetStatusBarVisible(settings.StatusBarVisible);
        _sessionRestoreEnabled = settings.SessionRestore;
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
            StatusBarVisible = StatusBarMenuItem.IsChecked,
            SessionRestore = _sessionRestoreEnabled
        });
    }

    private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        OpenSettingsWindow();
    }

    private void OpenSettingsWindow()
    {
        new SettingsWindow(this).ShowDialog();
        SavePersistentSettings();
        EditorTextBox.Focus();
    }

    // Surface used by SettingsWindow; each change applies live and is persisted when the dialog closes.
    internal string EditorFontFamilyName => _editorFontFamilyName;

    internal double EditorBaseFontSize => _editorBaseFontSize;

    internal bool IsWordWrapEnabled => WordWrapMenuItem.IsChecked;

    internal bool IsStatusBarVisible => StatusBarMenuItem.IsChecked;

    internal bool IsSessionRestoreEnabled => _sessionRestoreEnabled;

    internal bool IsCaptureExclusionRequested => _captureExclusionRequested;

    internal void ChangeEditorFontFamily(string fontFamilyName) => SetEditorFontFamily(fontFamilyName);

    internal void ChangeEditorFontSize(double fontSize) => SetEditorFontSize(fontSize);

    internal void ChangeWordWrap(bool wrap)
    {
        SetWordWrap(wrap);
        UpdateStatusFromEditor();
    }

    internal void ChangeStatusBarVisible(bool visible) => SetStatusBarVisible(visible);

    internal void ChangeTheme(AppTheme theme) => ApplyTheme(theme);

    internal void ChangeSessionRestore(bool enabled) => _sessionRestoreEnabled = enabled;

    internal void ChangeCaptureExclusion(bool excluded)
    {
        CaptureExclusionMenuItem.IsChecked = excluded;
        CaptureExclusionMenuItem_Click(CaptureExclusionMenuItem, new RoutedEventArgs());
    }
}
