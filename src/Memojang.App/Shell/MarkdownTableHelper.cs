namespace Memojang.App.Shell;

public static class MarkdownTableHelper
{
    public static string CreateStarterTable(string lineEnding)
    {
        return string.Join(
            lineEnding,
            "| Column 1 | Column 2 |",
            "| --- | --- |",
            "|  |  |",
            string.Empty);
    }

    public static string InsertStarterTable(string text, int caretIndex, string lineEnding)
    {
        string table = CreateStarterTable(lineEnding);
        string prefix = caretIndex > 0 && !IsAtLineStart(text, caretIndex) ? lineEnding : string.Empty;
        string suffix = caretIndex < text.Length && !IsAtLineStart(text, caretIndex) ? lineEnding : string.Empty;

        return text.Insert(caretIndex, $"{prefix}{table}{suffix}");
    }

    public static bool TryAddRow(string text, int caretIndex, string lineEnding, out string updatedText)
    {
        string normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        string[] lines = normalized.Split('\n');
        int currentLine = GetLineIndex(text, caretIndex);

        int start = currentLine;
        while (start > 0 && IsTableRow(lines[start - 1]))
        {
            start--;
        }

        int end = currentLine;
        while (end + 1 < lines.Length && IsTableRow(lines[end + 1]))
        {
            end++;
        }

        if (start + 1 >= lines.Length || !IsTableDivider(lines[start + 1]) || currentLine < start || currentLine > end)
        {
            updatedText = text;
            return false;
        }

        int columnCount = ParseTableRow(lines[start]).Count;
        lines = lines.InsertAt(end + 1, CreateEmptyRow(columnCount));
        updatedText = string.Join(lineEnding, lines);
        return true;
    }

    private static bool IsAtLineStart(string text, int index)
    {
        return index == 0 || text[index - 1] is '\r' or '\n';
    }

    private static int GetLineIndex(string text, int caretIndex)
    {
        int line = 0;

        for (int i = 0; i < caretIndex && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
            }
            else if (text[i] == '\r')
            {
                line++;
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }
            }
        }

        return line;
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

    private static string CreateEmptyRow(int columnCount)
    {
        return $"| {string.Join(" | ", Enumerable.Repeat(string.Empty, columnCount))} |";
    }

    private static T[] InsertAt<T>(this T[] source, int index, T item)
    {
        var updated = new T[source.Length + 1];
        Array.Copy(source, 0, updated, 0, index);
        updated[index] = item;
        Array.Copy(source, index, updated, index + 1, source.Length - index);
        return updated;
    }
}
