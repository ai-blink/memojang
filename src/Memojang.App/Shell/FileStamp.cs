using System.IO;

namespace Memojang.App.Shell;

public enum ExternalChangeAction
{
    None,
    ReloadSilently,
    AskReload,
    ReportMissing
}

// A cheap "did the file change on disk" fingerprint checked when the window is activated, instead of a
// FileSystemWatcher per tab (Markdown Monster-style rules: clean docs reload quietly, dirty docs ask).
public sealed record FileStamp(DateTime LastWriteTimeUtc, long Length)
{
    public static FileStamp? Read(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            var info = new FileInfo(path);
            return info.Exists ? new FileStamp(info.LastWriteTimeUtc, info.Length) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static ExternalChangeAction Decide(FileStamp? known, FileStamp? current, bool hasUnsavedChanges)
    {
        if (known is null)
        {
            return ExternalChangeAction.None;
        }

        if (current is null)
        {
            return ExternalChangeAction.ReportMissing;
        }

        if (known == current)
        {
            return ExternalChangeAction.None;
        }

        return hasUnsavedChanges ? ExternalChangeAction.AskReload : ExternalChangeAction.ReloadSilently;
    }
}
