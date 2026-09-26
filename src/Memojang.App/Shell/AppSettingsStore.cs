using System.IO;
using System.Text;
using System.Text.Json;

namespace Memojang.App.Shell;

public sealed class AppSettings
{
    public List<string> RecentFiles { get; init; } = [];

    public bool CaptureExclusionEnabled { get; init; }

    public string Theme { get; init; } = "System";

    public string EditorFontFamily { get; init; } = AppSettingsStore.DefaultFontFamily;

    public double EditorFontSize { get; init; } = AppSettingsStore.DefaultFontSize;

    public double ZoomPercent { get; init; } = 100;

    public bool WordWrap { get; init; } = true;

    public bool SessionRestore { get; init; } = true;
}

public sealed class AppSettingsStore
{
    public const int MaxRecentFiles = 8;
    public const string DefaultFontFamily = "Consolas";
    public const double DefaultFontSize = 14;
    public const double MinFontSize = 8;
    public const double MaxFontSize = 72;
    public const double MinZoomPercent = 50;
    public const double MaxZoomPercent = 200;

    private static readonly string[] KnownThemes = ["System", "Light", "Dark"];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public AppSettingsStore(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Memojang",
            "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(_settingsPath, Encoding.UTF8);
            return Normalize(JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppDiagnostics.Log($"App settings load failure path={_settingsPath} {ex}");
            return new AppSettings();
        }
    }

    public bool Save(AppSettings settings)
    {
        try
        {
            string? directoryPath = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonSerializer.Serialize(Normalize(settings), SerializerOptions);
            File.WriteAllText(_settingsPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppDiagnostics.Log($"App settings save failure path={_settingsPath} {ex}");
            return false;
        }
    }

    private static AppSettings Normalize(AppSettings? settings)
    {
        if (settings is null)
        {
            return new AppSettings();
        }

        List<string> recentFiles = (settings.RecentFiles ?? [])
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxRecentFiles)
            .ToList();

        return new AppSettings
        {
            RecentFiles = recentFiles,
            CaptureExclusionEnabled = settings.CaptureExclusionEnabled,
            Theme = KnownThemes.FirstOrDefault(theme => theme.Equals(settings.Theme, StringComparison.OrdinalIgnoreCase)) ?? "System",
            EditorFontFamily = string.IsNullOrWhiteSpace(settings.EditorFontFamily) ? DefaultFontFamily : settings.EditorFontFamily,
            EditorFontSize = ClampOrDefault(settings.EditorFontSize, MinFontSize, MaxFontSize, DefaultFontSize),
            ZoomPercent = ClampOrDefault(settings.ZoomPercent, MinZoomPercent, MaxZoomPercent, 100),
            WordWrap = settings.WordWrap,
            SessionRestore = settings.SessionRestore
        };
    }

    private static double ClampOrDefault(double value, double min, double max, double fallback)
    {
        return double.IsFinite(value) && value > 0 ? Math.Clamp(value, min, max) : fallback;
    }
}
