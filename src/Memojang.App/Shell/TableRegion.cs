using System.Text;

namespace Memojang.App.Shell;

// The pipe table around the caret: contiguous table rows whose second line is a divider (| --- | :-: |).
internal sealed class TableRegion
{
    public int Start { get; private init; }
    public int Length { get; private init; }
    public string LineEnding { get; private init; } = "\r\n";
    public List<List<string>> Rows { get; private init; } = [];
    public List<TableAlignment> Alignments { get; private init; } = [];
    public int CaretRow { get; private init; }
    public int CaretColumn { get; private init; }

    public static bool TryLocate(string text, int caretIndex, out TableRegion region)
    {
        region = new TableRegion();
        List<(int Start, int Length, int EndingLength)> lines = SplitLines(text);
        int caretLine = lines.FindLastIndex(line => line.Start <= caretIndex);
        if (caretLine < 0 || !IsTableRow(LineText(text, lines[caretLine])))
        {
            return false;
        }

        int first = caretLine;
        while (first > 0 && IsTableRow(LineText(text, lines[first - 1])))
        {
            first--;
        }

        int last = caretLine;
        while (last + 1 < lines.Count && IsTableRow(LineText(text, lines[last + 1])))
        {
            last++;
        }

        if (last == first || !IsDivider(LineText(text, lines[first + 1])))
        {
            return false;
        }

        var rows = new List<List<string>>();
        for (int line = first; line <= last; line++)
        {
            if (line != first + 1)
            {
                rows.Add(ParseCells(LineText(text, lines[line])));
            }
        }

        List<string> dividerCells = ParseCells(LineText(text, lines[first + 1]));
        int columnCount = Math.Max(rows.Max(cells => cells.Count), dividerCells.Count);
        rows.ForEach(cells => cells.AddRange(Enumerable.Repeat(string.Empty, columnCount - cells.Count)));
        List<TableAlignment> alignments = Enumerable.Range(0, columnCount)
            .Select(column => column < dividerCells.Count ? ParseAlignment(dividerCells[column]) : TableAlignment.None)
            .ToList();

        // The divider line itself counts as the header row for caret purposes.
        int caretRow = caretLine <= first + 1 ? 0 : caretLine - first - 1;
        string caretLineText = LineText(text, lines[caretLine]);
        string beforeCaret = caretLineText[..Math.Min(caretIndex - lines[caretLine].Start, caretLineText.Length)];
        int pipes = CountUnescapedPipes(beforeCaret);
        int caretColumn = caretLineText.TrimStart().StartsWith('|') ? pipes - 1 : pipes;

        (int Start, int Length, int EndingLength) lastLine = lines[last];
        int start = lines[first].Start;
        region = new TableRegion
        {
            Start = start,
            Length = lastLine.Start + lastLine.Length - start,
            LineEnding = lines[first].EndingLength > 0
                ? text.Substring(lines[first].Start + lines[first].Length, lines[first].EndingLength)
                : "\r\n",
            Rows = rows,
            Alignments = alignments,
            CaretRow = caretRow,
            CaretColumn = Math.Clamp(caretColumn, 0, columnCount - 1)
        };
        return true;
    }

    private static List<(int Start, int Length, int EndingLength)> SplitLines(string text)
    {
        var lines = new List<(int, int, int)>();
        int lineStart = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] is '\r' or '\n')
            {
                int endingLength = text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n' ? 2 : 1;
                lines.Add((lineStart, i - lineStart, endingLength));
                i += endingLength - 1;
                lineStart = i + 1;
            }
        }

        lines.Add((lineStart, text.Length - lineStart, 0));
        return lines;
    }

    private static string LineText(string text, (int Start, int Length, int EndingLength) line) =>
        text.Substring(line.Start, line.Length);

    private static bool IsTableRow(string line) => CountUnescapedPipes(line) >= 1 && ParseCells(line).Count > 1;

    private static bool IsDivider(string line)
    {
        List<string> cells = ParseCells(line);
        return cells.Count > 1 && cells.All(cell => cell.Contains('-') && cell.All(c => c is '-' or ':' or ' '));
    }

    private static TableAlignment ParseAlignment(string cell) => (cell.StartsWith(':'), cell.EndsWith(':')) switch
    {
        (true, true) => TableAlignment.Center,
        (true, false) => TableAlignment.Left,
        (false, true) => TableAlignment.Right,
        _ => TableAlignment.None
    };

    private static List<string> ParseCells(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.StartsWith('|'))
        {
            trimmed = trimmed[1..];
        }

        if (trimmed.EndsWith('|') && !trimmed.EndsWith("\\|", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^1];
        }

        var cells = new List<string>();
        var current = new StringBuilder();
        for (int i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] == '|' && (i == 0 || trimmed[i - 1] != '\\'))
            {
                cells.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(trimmed[i]);
            }
        }

        cells.Add(current.ToString().Trim());
        return cells;
    }

    private static int CountUnescapedPipes(string text) =>
        text.Where((c, i) => c == '|' && (i == 0 || text[i - 1] != '\\')).Count();
}
