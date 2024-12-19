using AET.SteamAbstraction.Vdf.Linq;
using Xunit;

namespace AET.SteamAbstraction.Test.Vdf;

public class VValueFacts
{
    [Fact]
    public void DeepCloneWorksCorrectly()
    {
        var original = new VValue("value1");

        var clone = original.DeepClone() as VValue;
        clone.Value = "value2";

        Assert.True(original.Value.Equals("value1"));
    }

    [Fact]
    public void DeepEqualsSucceedsCorrectly()
    {
        var val1 = new VValue("value1");
        var val2 = new VValue("value1");

        Assert.True(VToken.DeepEquals(val1, val2));
    }

    [Fact]
    public void DeepEqualsFailsCorrectly()
    {
        var val1 = new VValue("value1");
        var val2 = new VValue("value2");

        Assert.False(VToken.DeepEquals(val1, val2));
    }
}