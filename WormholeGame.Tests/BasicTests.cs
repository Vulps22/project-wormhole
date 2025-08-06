using Xunit;

namespace WormholeGame.Tests;

public class BasicTests
{
    [Fact]
    public void TrueIsTrue()
    {
        Assert.True(true);
    }

    [Fact]
    public void BasicAssertionWorks()
    {
        Assert.Equal(42, 40 + 2);
    }
}
