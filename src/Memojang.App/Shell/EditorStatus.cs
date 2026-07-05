namespace Memojang.App.Shell;

using Memojang.App.Localization;

public sealed record EditorStatus(
    int Line,
    int Column,
    bool HasUnsavedChanges,
    string EncodingName = "UTF-8",
    string LineEndingName = "CRLF")
{
    public string CaretText => UiText.Format("StatusCaret", Line, Column);

    public string ChangeText => UiText.Get(HasUnsavedChanges ? "StatusUnsavedChanges" : "StatusNoChanges");

    public static EditorStatus Initial => new(1, 1, false);
}
