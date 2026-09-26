namespace Memojang.Tests;

using Memojang.App.Shell;

public class FileStampTests
{
    private static readonly FileStamp Original = new(new DateTime(2026, 9, 27, 0, 0, 0, DateTimeKind.Utc), 10);
    private static readonly FileStamp Changed = new(new DateTime(2026, 9, 27, 0, 1, 0, DateTimeKind.Utc), 12);

    [Fact]
    public void Decide_UnchangedFile_DoesNothing()
    {
        Assert.Equal(ExternalChangeAction.None, FileStamp.Decide(Original, Original with { }, hasUnsavedChanges: true));
    }

    [Fact]
    public void Decide_ChangedCleanDocument_ReloadsSilently()
    {
        Assert.Equal(ExternalChangeAction.ReloadSilently, FileStamp.Decide(Original, Changed, hasUnsavedChanges: false));
    }

    [Fact]
    public void Decide_ChangedDirtyDocument_Asks()
    {
        Assert.Equal(ExternalChangeAction.AskReload, FileStamp.Decide(Original, Changed, hasUnsavedChanges: true));
    }

    [Fact]
    public void Decide_DeletedFile_ReportsMissing()
    {
        Assert.Equal(ExternalChangeAction.ReportMissing, FileStamp.Decide(Original, null, hasUnsavedChanges: false));
    }

    [Fact]
    public void Decide_UnsavedNewDocument_DoesNothing()
    {
        Assert.Equal(ExternalChangeAction.None, FileStamp.Decide(null, null, hasUnsavedChanges: true));
    }

    [Fact]
    public void Read_ReflectsDiskChanges()
    {
        string path = Path.Combine(Path.GetTempPath(), $"memojang-stamp-{Guid.NewGuid():N}.txt");
        try
        {
            File.WriteAllText(path, "one");
            FileStamp? first = FileStamp.Read(path);
            File.WriteAllText(path, "three");
            File.SetLastWriteTimeUtc(path, first!.LastWriteTimeUtc.AddSeconds(2));

            Assert.NotEqual(first, FileStamp.Read(path));
            Assert.Null(FileStamp.Read(path + ".missing"));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
