namespace Memojang.App.Shell;

public sealed record EditorStatus(
    int Line,
    int Column,
    bool HasUnsavedChanges,
    string EncodingName = "UTF-8",
    string LineEndingName = "CRLF")
{
    public string CaretText => $"Ln {Line}, Col {Column}";

    public string ChangeText => HasUnsavedChanges ? "Unsaved changes" : "No changes";

    public static EditorStatus Initial => new(1, 1, false);
}
