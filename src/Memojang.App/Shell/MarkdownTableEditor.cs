using System.Text;

namespace Memojang.App.Shell;

public enum TableAlignment
{
    None,
    Left,
    Center,
    Right
}

public enum TableCommand
{
    Format,
    NextCell,
    PreviousCell,
    AddRowBelow,
    DeleteRow,
    InsertColumnLeft,
    InsertColumnRight,
    DeleteColumn,
    AlignLeft,
    AlignCenter,
    AlignRight
}

// Replaces only the table's own text range, so text outside the table and its line endings stay untouched
// and the caller can apply it as one undoable selection replacement.
public sealed record TableEdit(int Start, int Length, string Replacement, int CaretIndex);

// Pipe-table editing in the source (Advanced Tables style): every command re-aligns the table and places the
// caret at the end of the target cell. Row 0 is the header; the divider is regenerated from alignments.
public static class MarkdownTableEditor
{
    public static bool IsInTable(string text, int caretIndex) => TableRegion.TryLocate(text, caretIndex, out _);

    public static bool TryApply(string text, int caretIndex, TableCommand command, out TableEdit edit)
    {
        edit = new TableEdit(0, 0, string.Empty, caretIndex);
        if (!TableRegion.TryLocate(text, caretIndex, out TableRegion region))
        {
            return false;
        }

        List<List<string>> rows = region.Rows;
        List<TableAlignment> alignments = region.Alignments;
        int row = region.CaretRow;
        int column = region.CaretColumn;
        int columnCount = alignments.Count;

        switch (command)
        {
            case TableCommand.NextCell:
                if (++column >= columnCount)
                {
                    column = 0;
                    if (++row >= rows.Count)
                    {
                        rows.Add(EmptyRow(columnCount));
                    }
                }

                break;
            case TableCommand.PreviousCell:
                if (--column < 0)
                {
                    column = row > 0 ? columnCount - 1 : 0;
                    row = Math.Max(0, row - 1);
                }

                break;
            case TableCommand.AddRowBelow:
                rows.Insert(row + 1, EmptyRow(columnCount));
                row++;
                break;
            case TableCommand.DeleteRow:
                if (row == 0)
                {
                    return false;
                }

                rows.RemoveAt(row);
                row = Math.Min(row, rows.Count - 1);
                break;
            case TableCommand.InsertColumnLeft:
            case TableCommand.InsertColumnRight:
                int insertAt = command == TableCommand.InsertColumnLeft ? column : column + 1;
                rows.ForEach(cells => cells.Insert(insertAt, string.Empty));
                alignments.Insert(insertAt, TableAlignment.None);
                column = insertAt;
                break;
            case TableCommand.DeleteColumn:
                if (columnCount == 1)
                {
                    return false;
                }

                rows.ForEach(cells => cells.RemoveAt(column));
                alignments.RemoveAt(column);
                column = Math.Min(column, alignments.Count - 1);
                break;
            case TableCommand.AlignLeft:
                alignments[column] = TableAlignment.Left;
                break;
            case TableCommand.AlignCenter:
                alignments[column] = TableAlignment.Center;
                break;
            case TableCommand.AlignRight:
                alignments[column] = TableAlignment.Right;
                break;
        }

        (string replacement, int caretOffset) = Render(rows, alignments, region.LineEnding, row, column);
        edit = new TableEdit(region.Start, region.Length, replacement, region.Start + caretOffset);
        return true;
    }

    private static List<string> EmptyRow(int columnCount) => Enumerable.Repeat(string.Empty, columnCount).ToList();

    private static (string Text, int CaretOffset) Render(
        List<List<string>> rows,
        List<TableAlignment> alignments,
        string lineEnding,
        int targetRow,
        int targetColumn)
    {
        int[] widths = alignments
            .Select((_, column) => Math.Max(3, rows.Max(cells => DisplayWidth(cells[column]))))
            .ToArray();

        var builder = new StringBuilder();
        int caretOffset = 0;
        for (int row = 0; row < rows.Count; row++)
        {
            if (row == 1)
            {
                builder.Append(RenderDivider(alignments, widths)).Append(lineEnding);
            }

            builder.Append('|');
            for (int column = 0; column < widths.Length; column++)
            {
                string cell = rows[row][column];
                int padding = widths[column] - DisplayWidth(cell);
                int leftPadding = alignments[column] switch
                {
                    TableAlignment.Right => padding,
                    TableAlignment.Center => padding / 2,
                    _ => 0
                };

                builder.Append(' ').Append(' ', leftPadding).Append(cell);
                if (row == targetRow && column == targetColumn)
                {
                    caretOffset = builder.Length;
                }

                builder.Append(' ', padding - leftPadding).Append(" |");
            }

            if (row < rows.Count - 1 || rows.Count == 1)
            {
                builder.Append(lineEnding);
            }
        }

        if (rows.Count == 1)
        {
            builder.Append(RenderDivider(alignments, widths));
        }

        return (builder.ToString(), caretOffset);
    }

    private static string RenderDivider(List<TableAlignment> alignments, int[] widths)
    {
        IEnumerable<string> cells = alignments.Select((alignment, column) => alignment switch
        {
            TableAlignment.Left => ":" + new string('-', widths[column] - 1),
            TableAlignment.Right => new string('-', widths[column] - 1) + ":",
            TableAlignment.Center => ":" + new string('-', widths[column] - 2) + ":",
            _ => new string('-', widths[column])
        });

        return $"| {string.Join(" | ", cells)} |";
    }

    // Monospace fonts draw Hangul and CJK ideographs at double width; counting them as 2 keeps columns aligned.
    internal static int DisplayWidth(string text)
    {
        int width = 0;
        foreach (char character in text)
        {
            width += character switch
            {
                >= 'ᄀ' and <= 'ᅟ' => 2,
                >= '⺀' and <= '꓏' => 2,
                >= '가' and <= '힣' => 2,
                >= '豈' and <= '﫿' => 2,
                >= '＀' and <= '｠' => 2,
                >= '￠' and <= '￦' => 2,
                _ => 1
            };
        }

        return width;
    }
}
