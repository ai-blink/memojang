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

    [Theory]
    [InlineData(DocumentMode.PlainText, ".txt")]
    [InlineData(DocumentMode.Markdown, ".md")]
    public void DocumentModeDetector_ReturnsDefaultSaveExtension(DocumentMode mode, string expectedExtension)
    {
        Assert.Equal(expectedExtension, DocumentModeDetector.GetDefaultExtension(mode));
    }

    [Theory]
    [InlineData(DocumentMode.PlainText, 1)]
    [InlineData(DocumentMode.Markdown, 2)]
    public void DocumentModeDetector_ReturnsSaveDialogFilterIndex(DocumentMode mode, int expectedFilterIndex)
    {
        Assert.Equal(expectedFilterIndex, DocumentModeDetector.GetSaveDialogFilterIndex(mode));
    }
}
