using System.IO;
using System.Text;
using System.Text.Json;

namespace Memojang.App.Shell;

public sealed record SessionTab(
    string? FilePath,
    string UntitledName,
    string Mode,
    string? UnsavedText,
    string EncodingKey,
    string LineEnding,
    int CaretIndex);

public sealed class SessionSnapshot
{
    public List<SessionTab> Tabs { get; init; } = [];

    public int ActiveIndex { get; init; }
}

// Hot exit: open tabs and unsaved text live here, never in the user's files. Clean file-backed tabs store only
// the path and are re-read from disk on restore.
public sealed class SessionStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _sessionPath;

    public SessionStore(string? sessionPath = null)
    {
        _sessionPath = sessionPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Memojang",
            "session.json");
    }

    public SessionSnapshot? Load()
    {
        try
        {
            return File.Exists(_sessionPath)
                ? JsonSerializer.Deserialize<SessionSnapshot>(File.ReadAllText(_sessionPath, Encoding.UTF8), SerializerOptions)
                : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppDiagnostics.Log($"Session load failure path={_sessionPath} {ex}");
            return null;
        }
    }

    public bool Save(SessionSnapshot snapshot)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_sessionPath)!);

            // Write-then-move so a crash mid-write never leaves a truncated session behind.
            string tempPath = _sessionPath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(snapshot, SerializerOptions), new UTF8Encoding(false));
            File.Move(tempPath, _sessionPath, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            AppDiagnostics.Log($"Session save failure path={_sessionPath} {ex}");
            return false;
        }
    }

    public void Clear()
    {
        try
        {
            File.Delete(_sessionPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDiagnostics.Log($"Session clear failure path={_sessionPath} {ex}");
        }
    }
}
