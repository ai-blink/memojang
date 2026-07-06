namespace Memojang.App.Shell;

using System.Text.RegularExpressions;

public static class MarkdownPreviewParser
{
    private static readonly Regex ImageRegex = new(
        @"^!\[(?<alt>[^\]]*)\]\((?<source>[^)]+)\)\s*$",
        RegexOptions.Compiled);

    public static IReadOnlyList<MarkdownPreviewBlock> Parse(string markdown)
    {
        var blocks = new List<MarkdownPreviewBlock>();
        string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (TryParseFence(line, out string language))
            {
                var codeLines = new List<string>();
                i++;

                while (i < lines.Length && !IsClosingFence(lines[i]))
                {
                    codeLines.Add(lines[i]);
                    i++;
                }

                blocks.Add(new MarkdownPreviewBlock(
                    MarkdownPreviewBlockKind.CodeBlock,
                    string.Join('\n', codeLines),
                    CodeLanguage: language));
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (TryParseEmptyHeading(line, out int emptyHeadingLevel, out string emptyHeadingMarker))
            {
                blocks.Add(new MarkdownPreviewBlock(
                    MarkdownPreviewBlockKind.EmptyHeading,
                    emptyHeadingMarker,
                    emptyHeadingLevel));
                continue;
            }

            if (IsThematicBreak(line))
            {
                blocks.Add(new MarkdownPreviewBlock(MarkdownPreviewBlockKind.ThematicBreak, string.Empty));
                continue;
            }

            if (TryParseHeading(line, out int headingLevel, out string headingText))
            {
                blocks.Add(new MarkdownPreviewBlock(MarkdownPreviewBlockKind.Heading, headingText, headingLevel));
                continue;
            }

            if (TryParseImage(line, out string altText, out string imageSource))
            {
                blocks.Add(new MarkdownPreviewBlock(
                    MarkdownPreviewBlockKind.Image,
                    altText,
                    ImageAltText: altText,
                    ImageSource: imageSource));
                continue;
            }

            if (TryParseUnorderedListItem(line, out int listLevel, out string listText))
            {
                var listLines = new List<ParsedListLine>
                {
                    new(listText, listLevel)
                };

                i++;
                while (i < lines.Length && TryParseUnorderedListItem(lines[i], out listLevel, out listText))
                {
                    listLines.Add(new ParsedListLine(listText, listLevel));
                    i++;
                }

                i--;
                blocks.Add(new MarkdownPreviewBlock(
                    MarkdownPreviewBlockKind.UnorderedList,
                    string.Empty,
                    ListItems: BuildListItems(listLines)));
                continue;
            }

            if (IsTableStart(lines, i))
            {
                var rows = new List<IReadOnlyList<string>> { ParseTableRow(lines[i]) };
                i += 2;

                while (i < lines.Length && IsTableRow(lines[i]))
                {
                    rows.Add(ParseTableRow(lines[i]));
                    i++;
                }

                i--;
                blocks.Add(new MarkdownPreviewBlock(MarkdownPreviewBlockKind.Table, string.Empty, TableRows: rows));
                continue;
            }

            blocks.Add(new MarkdownPreviewBlock(MarkdownPreviewBlockKind.Paragraph, line.Trim()));
        }

        return blocks;
    }

    private static bool TryParseHeading(string line, out int level, out string text)
    {
        level = 0;
        text = string.Empty;

        while (level < line.Length && level < 6 && line[level] == '#')
        {
            level++;
        }

        if (level == 0 || level >= line.Length || line[level] != ' ')
        {
            level = 0;
            return false;
        }

        text = line[(level + 1)..].Trim();
        return text.Length > 0;
    }

    private static bool TryParseEmptyHeading(string line, out int level, out string marker)
    {
        marker = line.Trim();
        level = marker.Length;

        return level is >= 1 and <= 6
            && marker.All(character => character == '#');
    }

    private static bool TryParseFence(string line, out string language)
    {
        string trimmed = line.Trim();
        language = string.Empty;

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return false;
        }

        language = trimmed[3..].Trim();
        return true;
    }

    private static bool IsClosingFence(string line)
    {
        return line.Trim().StartsWith("```", StringComparison.Ordinal);
    }

    private static bool IsThematicBreak(string line)
    {
        string marker = new(line.Trim().Where(character => character != ' ').ToArray());
        return marker.Length >= 3
            && (marker.All(character => character == '*')
                || marker.All(character => character == '-')
                || marker.All(character => character == '_'));
    }

    private static bool TryParseImage(string line, out string altText, out string source)
    {
        Match match = ImageRegex.Match(line.Trim());
        altText = string.Empty;
        source = string.Empty;

        if (!match.Success)
        {
            return false;
        }

        altText = match.Groups["alt"].Value.Trim();
        source = match.Groups["source"].Value.Trim();
        return source.Length > 0;
    }

    private static bool TryParseUnorderedListItem(string line, out int level, out string text)
    {
        level = 0;
        text = string.Empty;

        int indent = CountIndent(line);
        int markerIndex = line.TakeWhile(char.IsWhiteSpace).Count();
        if (markerIndex >= line.Length || line[markerIndex] != '-')
        {
            return false;
        }

        int contentIndex = markerIndex + 1;
        if (contentIndex >= line.Length || !char.IsWhiteSpace(line[contentIndex]))
        {
            return false;
        }

        text = line[(contentIndex + 1)..].Trim();
        level = Math.Max(1, indent / 2 + 1);
        return text.Length > 0;
    }

    private static int CountIndent(string line)
    {
        int indent = 0;
        foreach (char character in line)
        {
            if (character == ' ')
            {
                indent++;
                continue;
            }

            if (character == '\t')
            {
                indent += 4;
                continue;
            }

            break;
        }

        return indent;
    }

    private static IReadOnlyList<MarkdownPreviewListItem> BuildListItems(IReadOnlyList<ParsedListLine> lines)
    {
        var roots = new List<ListItemBuilder>();
        var stack = new List<ListItemBuilder>();

        foreach (ParsedListLine line in lines)
        {
            var item = new ListItemBuilder(line.Text, line.Level);
            while (stack.Count > 0 && stack[^1].Level >= item.Level)
            {
                stack.RemoveAt(stack.Count - 1);
            }

            if (stack.Count == 0)
            {
                roots.Add(item);
            }
            else
            {
                stack[^1].Children.Add(item);
            }

            stack.Add(item);
        }

        return roots.Select(ConvertListItem).ToArray();
    }

    private static MarkdownPreviewListItem ConvertListItem(ListItemBuilder item)
    {
        return new MarkdownPreviewListItem(
            item.Text,
            item.Level,
            item.Children.Select(ConvertListItem).ToArray());
    }

    private static bool IsTableStart(string[] lines, int index)
    {
        return index + 1 < lines.Length && IsTableRow(lines[index]) && IsTableDivider(lines[index + 1]);
    }

    private static bool IsTableRow(string line)
    {
        return line.Contains('|', StringComparison.Ordinal) && ParseTableRow(line).Count > 1;
    }

    private static bool IsTableDivider(string line)
    {
        var cells = ParseTableRow(line);
        return cells.Count > 1 && cells.All(cell => cell.Length > 0 && cell.All(character => character is '-' or ':' or ' '));
    }

    private static IReadOnlyList<string> ParseTableRow(string line)
    {
        return line.Trim().Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();
    }

    private sealed record ParsedListLine(string Text, int Level);

    private sealed class ListItemBuilder(string text, int level)
    {
        public string Text { get; } = text;

        public int Level { get; } = level;

        public List<ListItemBuilder> Children { get; } = [];
    }
}
