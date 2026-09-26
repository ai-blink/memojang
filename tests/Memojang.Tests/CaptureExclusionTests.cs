namespace Memojang.Tests;

using Memojang.App.WindowsInterop;

public class CaptureExclusionTests
{
    [Fact]
    public void Classify_UsesVerifiedExcludeAffinity()
    {
        var result = CaptureExclusion.Classify(true, CaptureExclusion.WdaExcludeFromCapture, null);

        Assert.Equal("PrivacyStatusExcluded", result.StatusKey);
        Assert.True(result.IsProtected);
    }

    [Fact]
    public void Classify_MonitorFallbackIsLimited()
    {
        var result = CaptureExclusion.Classify(true, CaptureExclusion.WdaMonitor, null);

        Assert.Equal("PrivacyStatusLimited", result.StatusKey);
    }

    [Fact]
    public void Classify_RequestedButWindowsReportsNone_IsUnavailableWithError()
    {
        var result = CaptureExclusion.Classify(true, CaptureExclusion.WdaNone, 8);

        Assert.Equal("PrivacyStatusExclusionUnavailable", result.StatusKey);
        Assert.False(result.IsProtected);
        Assert.Equal(8, result.ErrorCode);
    }

    [Fact]
    public void Classify_TurnedOffButStillExcluded_ReportsActualState()
    {
        var result = CaptureExclusion.Classify(false, CaptureExclusion.WdaExcludeFromCapture, 5);

        Assert.Equal("PrivacyStatusExcluded", result.StatusKey);
    }

    [Fact]
    public void Classify_TurnedOff_IsVisible()
    {
        Assert.Equal("PrivacyStatusVisible", CaptureExclusion.Classify(false, CaptureExclusion.WdaNone, null).StatusKey);
    }

    [Fact]
    public void Classify_UnreadableAffinity_IsUnavailable()
    {
        var result = CaptureExclusion.Classify(true, null, null);

        Assert.Equal("PrivacyStatusUnavailable", result.StatusKey);
        Assert.False(result.IsProtected);
    }
}
