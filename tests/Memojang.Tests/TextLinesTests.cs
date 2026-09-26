namespace Memojang.Tests;

using Memojang.App.Shell;

public class TextLinesTests
{
    [Theory]
    [InlineData("a\r\nbc\r\nd", 4, 2, 2)]
    [InlineData("a\nbc\nd", 3, 2, 2)]
    [InlineData("a\rbc\rd", 5, 3, 1)]
    [InlineData("abc", 0, 1, 1)]
    public void GetPosition_CountsLogicalLines(string text, int index, int line, int column)
    {
        Assert.Equal((line, column), TextLines.GetPosition(text, index));
    }

    [Fact]
    public void Count_TreatsCrlfAsOneBreak()
    {
        Assert.Equal(3, TextLines.Count("a\r\nb\nc"));
        Assert.Equal(1, TextLines.Count(string.Empty));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 3)]
    [InlineData(3, 7)]
    public void GetLineStartIndex_ReturnsFirstCharacterOfLine(int lineNumber, int expected)
    {
        Assert.Equal(expected, TextLines.GetLineStartIndex("a\r\nbc\r\nd", lineNumber));
    }
}
