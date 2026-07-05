namespace Memojang.App.Shell;

public static class MarkdownPreviewParser
{
    public static IReadOnlyList<MarkdownPreviewBlock> Parse(string markdown)
    {
        var blocks = new List<MarkdownPreviewBlock>();
        string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (TryParseHeading(line, out int headingLevel, out string headingText))
            {
                blocks.Add(new MarkdownPreviewBlock(MarkdownPreviewBlockKind.Heading, headingText, headingLevel));
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
}
