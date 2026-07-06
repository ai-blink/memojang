namespace Memojang.App.Shell;

public enum MarkdownPreviewBlockKind
{
    CodeBlock,
    EmptyHeading,
    Heading,
    Image,
    Paragraph,
    Table,
    ThematicBreak,
    UnorderedList
}

public sealed record MarkdownPreviewListItem(
    string Text,
    int Level,
    IReadOnlyList<MarkdownPreviewListItem> Children);

public sealed record MarkdownPreviewBlock(
    MarkdownPreviewBlockKind Kind,
    string Text,
    int HeadingLevel = 0,
    IReadOnlyList<IReadOnlyList<string>>? TableRows = null,
    IReadOnlyList<MarkdownPreviewListItem>? ListItems = null,
    string? CodeLanguage = null,
    string? ImageAltText = null,
    string? ImageSource = null);
