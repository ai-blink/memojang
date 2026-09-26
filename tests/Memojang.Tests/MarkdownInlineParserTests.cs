namespace Memojang.Tests;

using Memojang.App.Shell;

public class MarkdownInlineParserTests
{
    [Fact]
    public void Parse_RecognizesToolbarInsertedSyntax()
    {
        var spans = MarkdownInlineParser.Parse("a **b** *c* ~~d~~ [e](https://x.test) `f`");

        Assert.Equal(
            [
                MarkdownInlineKind.Text, MarkdownInlineKind.Bold, MarkdownInlineKind.Text,
                MarkdownInlineKind.Italic, MarkdownInlineKind.Text, MarkdownInlineKind.Strike,
                MarkdownInlineKind.Text, MarkdownInlineKind.Link, MarkdownInlineKind.Text, MarkdownInlineKind.Code
            ],
            spans.Select(span => span.Kind));
        Assert.Equal("c", spans[3].Text);
        Assert.Equal("e", spans[7].Text);
        Assert.Equal("https://x.test", spans[7].Url);
    }

    [Theory]
    [InlineData("2 * 3 * 4")]
    [InlineData("file_name_here")]
    [InlineData("**unclosed")]
    [InlineData("[label] (not a link)")]
    public void Parse_LeavesNonMarkupAsPlainText(string text)
    {
        var spans = MarkdownInlineParser.Parse(text);

        Assert.Single(spans);
        Assert.Equal(MarkdownInlineKind.Text, spans[0].Kind);
        Assert.Equal(text, spans[0].Text);
    }

    [Fact]
    public void Parse_CodeSpanProtectsInnerMarkers()
    {
        var spans = MarkdownInlineParser.Parse("`**raw**`");

        Assert.Single(spans);
        Assert.Equal(MarkdownInlineKind.Code, spans[0].Kind);
        Assert.Equal("**raw**", spans[0].Text);
    }
}
