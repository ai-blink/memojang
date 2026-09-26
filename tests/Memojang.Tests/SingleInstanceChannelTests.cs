namespace Memojang.Tests;

using Memojang.App.Shell;

public class SingleInstanceChannelTests
{
    [Fact]
    public void SecondChannel_IsNotPrimary()
    {
        string name = $"memojang-test-{Guid.NewGuid():N}";
        using var primary = new SingleInstanceChannel(name);
        using var secondary = new SingleInstanceChannel(name);

        Assert.True(primary.IsPrimary);
        Assert.False(secondary.IsPrimary);
    }

    [Fact]
    public async Task Secondary_ForwardsPathsToPrimary()
    {
        string name = $"memojang-test-{Guid.NewGuid():N}";
        using var primary = new SingleInstanceChannel(name);
        var received = new TaskCompletionSource<IReadOnlyList<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        primary.StartListening(paths => received.TrySetResult(paths));

        using var secondary = new SingleInstanceChannel(name);
        Assert.True(secondary.TrySendToPrimary([@"C:\docs\a.txt", @"C:\docs\b b.md"], TimeSpan.FromSeconds(5)));

        IReadOnlyList<string> paths = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal([@"C:\docs\a.txt", @"C:\docs\b b.md"], paths);
    }

    [Fact]
    public async Task Secondary_WithoutPaths_SendsEmptyRequest()
    {
        string name = $"memojang-test-{Guid.NewGuid():N}";
        using var primary = new SingleInstanceChannel(name);
        var received = new TaskCompletionSource<IReadOnlyList<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        primary.StartListening(paths => received.TrySetResult(paths));

        using var secondary = new SingleInstanceChannel(name);
        Assert.True(secondary.TrySendToPrimary([], TimeSpan.FromSeconds(5)));

        Assert.Empty(await received.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void Send_WithoutListeningPrimary_FailsWithinTimeout()
    {
        using var lonely = new SingleInstanceChannel($"memojang-test-{Guid.NewGuid():N}");

        Assert.False(lonely.TrySendToPrimary([@"C:\docs\a.txt"], TimeSpan.FromMilliseconds(200)));
    }
}
