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
        Assert.Equal("Capture status unavailable", result.DetailText);
    }

    [Fact]
    public void CaptureExclusion_DetailText_IncludesWin32ErrorWhenPresent()
    {
        var result = new App.WindowsInterop.CaptureExclusionResult(
            IsEnabled: true,
            IsProtected: false,
            StatusText: "Capture exclusion unavailable",
            ErrorCode: 5);

        Assert.Equal("Capture exclusion unavailable (Win32 5)", result.DetailText);
    }

    [Theory]
    [InlineData("alpha beta gamma", "beta", 0, 6)]
    [InlineData("alpha beta alpha", "alpha", 6, 11)]
    [InlineData("alpha beta alpha", "alpha", 16, 0)]
    [InlineData("alpha beta", "delta", 0, -1)]
    public void TextSearch_FindNext_FindsAndWraps(string text, string query, int startIndex, int expected)
    {
        int match = App.Shell.TextSearch.FindNext(text, query, startIndex, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(expected, match);
    }

    [Fact]
    public void TextSearch_ReplaceAll_ReplacesCaseInsensitiveMatches()
    {
        var result = App.Shell.TextSearch.ReplaceAll(
            "Alpha beta alpha",
            "alpha",
            "one",
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal("one beta one", result.Text);
        Assert.Equal(2, result.Count);
    }
}
