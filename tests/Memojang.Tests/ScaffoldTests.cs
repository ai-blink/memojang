namespace Memojang.Tests;

public class ScaffoldTests
{
    [Fact]
    public void App_UsesExpectedNamespace()
    {
        Assert.Equal("Memojang.App", typeof(App.App).Namespace);
    }
}
