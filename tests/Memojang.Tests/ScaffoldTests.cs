namespace Memojang.Tests;

public class ScaffoldTests
{
    [Fact]
    public void App_UsesExpectedNamespace()
    {
        Assert.Equal("Memojang.App", typeof(App.App).Namespace);
    }

    [Fact]
    public void EditorStatus_InitialState_MatchesEmptyDocumentShell()
    {
        var status = App.Shell.EditorStatus.Initial;

        Assert.Equal("Ln 1, Col 1", status.CaretText);
        Assert.Equal("No changes", status.ChangeText);
        Assert.Equal("UTF-8", status.EncodingName);
        Assert.Equal("CRLF", status.LineEndingName);
    }

    [Fact]
    public void EditorStatus_UnsavedChanges_UsesShellStatusText()
    {
        var status = new App.Shell.EditorStatus(3, 12, true);

        Assert.Equal("Ln 3, Col 12", status.CaretText);
        Assert.Equal("Unsaved changes", status.ChangeText);
    }

    [Theory]
    [InlineData("first\r\nsecond\r\n", "CRLF")]
    [InlineData("first\nsecond\n", "LF")]
    [InlineData("first\rsecond\r", "CR")]
    [InlineData("", "CRLF")]
    public void LineEnding_DetectsDominantLineEnding(string text, string expected)
    {
        Assert.Equal(expected, App.Shell.LineEnding.Detect(text));
    }

    [Fact]
    public void MarkdownPreviewParser_ParsesHeadingsParagraphsAndTables()
    {
        const string markdown = """
            # Title

            A short paragraph.

            | Name | Value |
            | --- | --- |
            | One | 1 |
            """;

        var blocks = App.Shell.MarkdownPreviewParser.Parse(markdown);

        Assert.Collection(
            blocks,
            heading =>
            {
                Assert.Equal(App.Shell.MarkdownPreviewBlockKind.Heading, heading.Kind);
                Assert.Equal(1, heading.HeadingLevel);
                Assert.Equal("Title", heading.Text);
            },
            paragraph =>
            {
                Assert.Equal(App.Shell.MarkdownPreviewBlockKind.Paragraph, paragraph.Kind);
                Assert.Equal("A short paragraph.", paragraph.Text);
            },
            table =>
            {
                Assert.Equal(App.Shell.MarkdownPreviewBlockKind.Table, table.Kind);
                Assert.NotNull(table.TableRows);
                Assert.Equal(2, table.TableRows.Count);
                Assert.Equal(["Name", "Value"], table.TableRows[0]);
                Assert.Equal(["One", "1"], table.TableRows[1]);
            });
    }

    [Fact]
    public void MarkdownTableHelper_InsertsStarterTableAtCaret()
    {
        string updated = App.Shell.MarkdownTableHelper.InsertStarterTable("Hello", 5, "\n");

        Assert.Contains("| Column 1 | Column 2 |", updated);
        Assert.Contains("| --- | --- |", updated);
        Assert.Contains("|  |  |", updated);
    }

    [Fact]
    public void MarkdownTableHelper_AddsEmptyRowInsideTable()
    {
        const string markdown = """
            | Name | Value |
            | --- | --- |
            | One | 1 |
            """;

        bool added = App.Shell.MarkdownTableHelper.TryAddRow(markdown, markdown.IndexOf("One", StringComparison.Ordinal), "\n", out string updated);

        Assert.True(added);
        Assert.Contains("|  |  |", updated);
    }

    [Fact]
    public void CaptureExclusion_ZeroHandle_ReturnsUnavailableStatus()
    {
        var result = App.WindowsInterop.CaptureExclusion.Apply(IntPtr.Zero, enabled: true);

        Assert.True(result.IsEnabled);
        Assert.False(result.IsProtected);
        Assert.Equal("Capture status unavailable", result.StatusText);
    }
}
