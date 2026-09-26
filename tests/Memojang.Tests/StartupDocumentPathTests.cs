namespace Memojang.Tests;

using Memojang.App.Shell;

public class StartupDocumentPathTests : IDisposable
{
    private readonly string _workspace = Path.Combine(Path.GetTempPath(), $"memojang-startup-{Guid.NewGuid():N}");

    public StartupDocumentPathTests()
    {
        Directory.CreateDirectory(_workspace);
    }

    [Fact]
    public void FromArgs_ReturnsEveryExistingFileInOrder()
    {
        string first = Path.Combine(_workspace, "first.txt");
        string second = Path.Combine(_workspace, "second.md");
        File.WriteAllText(first, "first");
        File.WriteAllText(second, "# second");

        IReadOnlyList<string> result = StartupDocumentPath.FromArgs(["--ignored", first, second, first]);

        Assert.Equal([first, second], result);
    }

    [Fact]
    public void FromArgs_IgnoresMissingPaths()
    {
        string existing = Path.Combine(_workspace, "note.md");
        File.WriteAllText(existing, "# note");

        IReadOnlyList<string> result = StartupDocumentPath.FromArgs([Path.Combine(_workspace, "missing.md"), existing]);

        Assert.Equal([existing], result);
    }

    [Fact]
    public void FromArgs_TrimsLiteralQuotes()
    {
        string existing = Path.Combine(_workspace, "quoted note.txt");
        File.WriteAllText(existing, "quoted");

        IReadOnlyList<string> result = StartupDocumentPath.FromArgs([$"\"{existing}\""]);

        Assert.Equal([existing], result);
    }

    [Fact]
    public void FromArgs_ResolvesRelativePathsToAbsolute()
    {
        string existing = Path.Combine(_workspace, "relative.txt");
        File.WriteAllText(existing, "relative");
        string relative = Path.GetRelativePath(Environment.CurrentDirectory, existing);

        IReadOnlyList<string> result = StartupDocumentPath.FromArgs([relative]);

        Assert.Equal([existing], result);
    }

    [Fact]
    public void FromArgs_ReturnsEmptyWhenNoFileExists()
    {
        IReadOnlyList<string> result = StartupDocumentPath.FromArgs(["", "   ", Path.Combine(_workspace, "missing.txt")]);

        Assert.Empty(result);
    }

    public void Dispose()
    {
        Directory.Delete(_workspace, recursive: true);
    }
}
