using Memojang.App.Shell;

namespace Memojang.Tests;

public sealed class AppSettingsStoreTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        "Memojang.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void SaveAndLoad_RoundTripsRecentFilesAndCapturePreference()
    {
        var store = new AppSettingsStore(Path.Combine(_testDirectory, "settings.json"));
        var settings = new AppSettings
        {
            RecentFiles = [@"C:\notes\first.md", @"C:\notes\second.txt"],
            CaptureExclusionEnabled = true
        };

        Assert.True(store.Save(settings));

        AppSettings loaded = store.Load();
        Assert.Equal(settings.RecentFiles, loaded.RecentFiles);
        Assert.True(loaded.CaptureExclusionEnabled);
    }

    [Fact]
    public void Load_NormalizesRecentFilesWithoutChangingTheirOrder()
    {
        var store = new AppSettingsStore(Path.Combine(_testDirectory, "settings.json"));
        string[] recentFiles =
        [
            @"C:\notes\first.md",
            " ",
            @"C:\NOTES\FIRST.md",
            @"C:\notes\second.md",
            @"C:\notes\third.md",
            @"C:\notes\fourth.md",
            @"C:\notes\fifth.md",
            @"C:\notes\sixth.md",
            @"C:\notes\seventh.md",
            @"C:\notes\eighth.md",
            @"C:\notes\ninth.md"
        ];

        Assert.True(store.Save(new AppSettings { RecentFiles = recentFiles.ToList() }));

        AppSettings loaded = store.Load();
        Assert.Equal(AppSettingsStore.MaxRecentFiles, loaded.RecentFiles.Count);
        Assert.Equal(@"C:\notes\first.md", loaded.RecentFiles[0]);
        Assert.Equal(@"C:\notes\eighth.md", loaded.RecentFiles[^1]);
    }

    [Fact]
    public void Load_WithInvalidJson_ReturnsDefaults()
    {
        string settingsPath = Path.Combine(_testDirectory, "settings.json");
        Directory.CreateDirectory(_testDirectory);
        File.WriteAllText(settingsPath, "{not-json");
        var store = new AppSettingsStore(settingsPath);

        AppSettings loaded = store.Load();

        Assert.Empty(loaded.RecentFiles);
        Assert.False(loaded.CaptureExclusionEnabled);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
}
