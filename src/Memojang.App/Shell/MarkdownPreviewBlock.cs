namespace Memojang.App.Shell;

public enum MarkdownPreviewBlockKind
{
    Heading,
    Paragraph,
    Table
}

public sealed record MarkdownPreviewBlock(
    MarkdownPreviewBlockKind Kind,
    string Text,
    int HeadingLevel = 0,
    IReadOnlyList<IReadOnlyList<string>>? TableRows = null);
