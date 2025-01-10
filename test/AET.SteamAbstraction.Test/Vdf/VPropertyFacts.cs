using AET.SteamAbstraction.Vdf.Linq;
using Xunit;

namespace AET.SteamAbstraction.Test.Vdf;

// Taken from https://github.com/shravan2x/Gameloop.Vdf

public class VPropertyFacts
{
    [Fact]
    public void DeepCloneWorksCorrectly()
    {
        var original = new VProperty("key1", new VObject
        {
            new VProperty("key2", new VValue("value2")),
        });

        var clone = original.DeepClone() as VProperty;
        clone!.Value = new VValue("value3");

        Assert.True(original.Value is VObject);
    }

    [Fact]
    public void DeepEqualsSucceedsCorrectly()
    {
        var prop1 = new VProperty("key1", new VValue("value1"));
        var prop2 = new VProperty("key1", new VValue("value1"));

        Assert.True(VToken.DeepEquals(prop1, prop2));
    }

    [Fact]
    public void DeepEqualsFailsCorrectly()
    {
        var prop1 = new VProperty("key1", new VValue("value1"));
        var prop2 = new VProperty("key2", new VValue("value1"));
        var prop3 = new VProperty("key1", new VValue("value2"));

        Assert.False(VToken.DeepEquals(prop1, prop2));
        Assert.False(VToken.DeepEquals(prop1, prop3));
    }
}