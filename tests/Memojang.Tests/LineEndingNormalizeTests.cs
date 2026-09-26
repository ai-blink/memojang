namespace Memojang.Tests;

using Memojang.App.Shell;

public class LineEndingNormalizeTests
{
    [Theory]
    [InlineData("a\r\nb\nc\rd", LineEnding.Lf, "a\nb\nc\nd")]
    [InlineData("a\r\nb\nc\rd", LineEnding.Crlf, "a\r\nb\r\nc\r\nd")]
    [InlineData("a\nb", LineEnding.Cr, "a\rb")]
    [InlineData("no breaks", LineEnding.Lf, "no breaks")]
    public void Normalize_ConvertsEveryBreakToTarget(string input, string lineEnding, string expected)
    {
        Assert.Equal(expected, LineEnding.Normalize(input, lineEnding));
    }

    [Fact]
    public void Normalize_ResultIsDetectedAsTarget()
    {
        Assert.Equal(LineEnding.Lf, LineEnding.Detect(LineEnding.Normalize("a\r\nb\r\nc", LineEnding.Lf)));
    }
}
