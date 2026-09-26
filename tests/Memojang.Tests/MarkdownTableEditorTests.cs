namespace Memojang.Tests;

using Memojang.App.Shell;

public class MarkdownTableEditorTests
{
    private const string Table = "| a | bb |\r\n| --- | --- |\r\n| c | d |";

    private static string Apply(string text, int caret, TableCommand command, out int newCaret)
    {
        Assert.True(MarkdownTableEditor.TryApply(text, caret, command, out TableEdit edit));
        newCaret = edit.CaretIndex;
        return text[..edit.Start] + edit.Replacement + text[(edit.Start + edit.Length)..];
    }

    [Fact]
    public void Format_AlignsColumnsAndKeepsSurroundingText()
    {
        string text = "before\r\n|a|bb|\r\n|-|-|\r\n|ccc|d|\r\nafter";

        string result = Apply(text, text.IndexOf("|a", StringComparison.Ordinal) + 1, TableCommand.Format, out _);

        Assert.Equal("before\r\n| a   | bb  |\r\n| --- | --- |\r\n| ccc | d   |\r\nafter", result);
    }

    [Fact]
    public void NextCell_MovesRightThenWrapsToNextRow()
    {
        string result = Apply(Table, 2, TableCommand.NextCell, out int caret);
        Assert.Equal("bb", result[(caret - 2)..caret]);

        result = Apply(result, caret, TableCommand.NextCell, out caret);
        Assert.Equal('c', result[caret - 1]);
    }

    [Fact]
    public void NextCell_FromLastCellAddsRow()
    {
        string result = Apply(Table, Table.Length - 2, TableCommand.NextCell, out _);

        Assert.Equal(4, result.Split("\r\n").Length);
        Assert.EndsWith("|     |     |", result);
    }

    [Fact]
    public void InsertAndDeleteColumn_RoundTrips()
    {
        string inserted = Apply(Table, 2, TableCommand.InsertColumnRight, out int caret);
        Assert.StartsWith("| a   |     | bb  |", inserted);

        string deleted = Apply(inserted, caret, TableCommand.DeleteColumn, out _);
        Assert.StartsWith("| a   | bb  |", deleted);
    }

    [Fact]
    public void DeleteRow_RefusesHeader()
    {
        Assert.False(MarkdownTableEditor.TryApply(Table, 2, TableCommand.DeleteRow, out _));
    }

    [Fact]
    public void AlignCenter_WritesDividerMarkers()
    {
        string result = Apply(Table, 2, TableCommand.AlignCenter, out _);

        Assert.Contains("| :-: | --- |", result);
    }

    [Fact]
    public void Format_CountsHangulAsDoubleWidth()
    {
        string text = "| 한글 | x |\n| - | - |\n| a | b |";

        string result = Apply(text, 2, TableCommand.Format, out _);

        Assert.Equal("| 한글 | x   |\n| ---- | --- |\n| a    | b   |", result);
    }

    [Theory]
    [InlineData("plain text")]
    [InlineData("| a | b |\n| c | d |")]
    public void TryApply_OutsideTable_ReturnsFalse(string text)
    {
        Assert.False(MarkdownTableEditor.IsInTable(text, 1));
    }
}
