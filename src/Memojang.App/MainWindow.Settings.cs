using Memojang.App.Shell;

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
    }

    private void SavePersistentSettings()
    {
        _settingsStore.Save(new AppSettings
        {
            RecentFiles = [.. _recentFilePaths],
            CaptureExclusionEnabled = _captureExclusionRequested
        });
    }
}
