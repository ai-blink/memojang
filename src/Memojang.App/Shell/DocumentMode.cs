using System.IO;
using Memojang.App.Localization;

namespace Memojang.App.Shell;

public enum DocumentMode
{
    PlainText,
    Markdown
}

public static class DocumentModeDetector
{
    public static DocumentMode FromPath(string? filePath)
    {
        string extension = Path.GetExtension(filePath ?? string.Empty);

        return extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase)
                ? DocumentMode.Markdown
                : DocumentMode.PlainText;
    }

    public static string GetStatusText(DocumentMode mode)
    {
        return UiText.Get(mode == DocumentMode.Markdown ? "StatusDocumentMarkdown" : "StatusDocumentText");
    }
}
