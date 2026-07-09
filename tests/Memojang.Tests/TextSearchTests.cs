namespace Memojang.Tests;

using Memojang.App.Shell;

public class TextSearchTests
{
    [Fact]
    public void FindPrevious_ReturnsPriorMatch()
    {
        int match = TextSearch.FindPrevious("alpha beta alpha", "alpha", 10, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(0, match);
    }

    [Fact]
    public void FindPrevious_WrapsFromBeginning()
    {
        int match = TextSearch.FindPrevious("alpha beta alpha", "alpha", -1, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(11, match);
    }

    [Fact]
    public void FindPrevious_ReturnsMinusOneWhenMissing()
    {
        int match = TextSearch.FindPrevious("alpha beta", "gamma", 9, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(-1, match);
    }
}
