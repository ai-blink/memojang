namespace Memojang.Tests;

using Memojang.App.Shell;

public sealed class SessionStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "Memojang.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SaveAndLoad_RoundTripsTabsAndUnsavedText()
    {
        var store = new SessionStore(Path.Combine(_directory, "session.json"));
        var snapshot = new SessionSnapshot
        {
            Tabs =
            [
                new SessionTab(@"C:\notes\a.md", "Untitled.md", "Markdown", null, "utf-8-bom", "LF", 3),
                new SessionTab(null, "Untitled 2.txt", "PlainText", "초안\r\n내용", "utf-8", "CRLF", 2)
            ],
            ActiveIndex = 1
        };

        Assert.True(store.Save(snapshot));
        SessionSnapshot? loaded = store.Load();

        Assert.NotNull(loaded);
        Assert.Equal(snapshot.Tabs, loaded!.Tabs);
        Assert.Equal(1, loaded.ActiveIndex);
        Assert.False(File.Exists(Path.Combine(_directory, "session.json.tmp")));
    }

    [Fact]
    public void Load_MissingOrCorruptSession_ReturnsNull()
    {
        string path = Path.Combine(_directory, "session.json");
        var store = new SessionStore(path);
        Assert.Null(store.Load());

        Directory.CreateDirectory(_directory);
        File.WriteAllText(path, "{ not json");
        Assert.Null(store.Load());
    }

    [Fact]
    public void Clear_RemovesSession()
    {
        var store = new SessionStore(Path.Combine(_directory, "session.json"));
        store.Save(new SessionSnapshot());

        store.Clear();

        Assert.Null(store.Load());
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-8-bom")]
    [InlineData("utf-16le")]
    [InlineData("utf-16be")]
    [InlineData("ansi-949")]
    public void EncodingKey_RoundTrips(string key)
    {
        Assert.Equal(key, TextFileEncoding.FromKey(key).Key);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
