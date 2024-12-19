using AET.SteamAbstraction.Vdf.Linq;
using Xunit;

namespace AET.SteamAbstraction.Test.Vdf;

public class VObjectFacts
{
    [Fact]
    public void DeepCloneWorksCorrectly()
    {
        var original = new VObject
        {
            new VProperty("key1", new VValue("value1")),
        };

        var clone = original.DeepClone() as VObject;
        clone["key1"] = new VValue("value2");

        Assert.True(((VValue)original["key1"]).Value.Equals("value1"));
    }

    [Fact]
    public void DeepEqualsSucceedsCorrectly()
    {
        var obj1 = new VObject
        {
            new VProperty("key1", new VValue("value1")),
            new VProperty("key2", new VValue("value2")),
        };

        var obj2 = new VObject
        {
            new VProperty("key1", new VValue("value1")),
            new VProperty("key2", new VValue("value2")),
        };

        Assert.True(VToken.DeepEquals(obj1, obj2));
    }

    [Fact]
    public void DeepEqualsFailsCorrectly()
    {
        var obj1 = new VObject
        {
            new VProperty("key1", new VValue("value1")),
            new VProperty("key2", new VValue("value2")),
        };

        var obj2 = new VObject
        {
            new VProperty("key1", new VValue("value1")),
            new VProperty("key2", new VValue("value3")),
        };

        Assert.False(VToken.DeepEquals(obj1, obj2));
    }
}