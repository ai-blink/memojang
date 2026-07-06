namespace Memojang.Tests;

using Memojang.App.Shell;

public class DocumentModeTests
{
    [Theory]
    [InlineData(null, DocumentMode.PlainText)]
    [InlineData("note.txt", DocumentMode.PlainText)]
    [InlineData("note", DocumentMode.PlainText)]
    [InlineData("note.md", DocumentMode.Markdown)]
    [InlineData("note.MD", DocumentMode.Markdown)]
    [InlineData("note.markdown", DocumentMode.Markdown)]
    public void DocumentModeDetector_UsesMarkdownExtensions(string? path, DocumentMode expected)
    {
        Assert.Equal(expected, DocumentModeDetector.FromPath(path));
    }
}
