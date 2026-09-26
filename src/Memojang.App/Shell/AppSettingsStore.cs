using System.IO;
using System.Text;
using System.Text.Json;

namespace Memojang.App.Shell;

public sealed class AppSettings
{
    public List<string> RecentFiles { get; init; } = [];

    public bool CaptureExclusionEnabled { get; init; }
}

public sealed class AppSettingsStore
{
    public const int MaxRecentFiles = 8;

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
            CaptureExclusionEnabled = settings.CaptureExclusionEnabled
        };
    }
}
