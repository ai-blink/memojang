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
    public void FromArgs_ReturnsFirstExistingFile()
    {
        string first = Path.Combine(_workspace, "first.txt");
        string second = Path.Combine(_workspace, "second.md");
        File.WriteAllText(first, "first");
        File.WriteAllText(second, "# second");

        string? result = StartupDocumentPath.FromArgs(["--ignored", first, second]);

        Assert.Equal(first, result);
    }

    [Fact]
    public void FromArgs_IgnoresMissingPaths()
    {
        string existing = Path.Combine(_workspace, "note.md");
        File.WriteAllText(existing, "# note");

        string? result = StartupDocumentPath.FromArgs([Path.Combine(_workspace, "missing.md"), existing]);

        Assert.Equal(existing, result);
    }

    [Fact]
    public void FromArgs_TrimsLiteralQuotes()
    {
        string existing = Path.Combine(_workspace, "quoted note.txt");
        File.WriteAllText(existing, "quoted");

        string? result = StartupDocumentPath.FromArgs([$"\"{existing}\""]);

        Assert.Equal(existing, result);
    }

    [Fact]
    public void FromArgs_ReturnsNullWhenNoFileExists()
    {
        string? result = StartupDocumentPath.FromArgs(["", "   ", Path.Combine(_workspace, "missing.txt")]);

        Assert.Null(result);
    }

    public void Dispose()
    {
        Directory.Delete(_workspace, recursive: true);
    }
}
