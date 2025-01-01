using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using System;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class FlagArgumentTest
{
    [Fact]
    public void Ctor_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TestFlagArg(null!, TestHelpers.RandomBool(), TestHelpers.RandomBool()));
        Assert.Throws<ArgumentException>(() => new TestFlagArg(string.Empty, TestHelpers.RandomBool(), TestHelpers.RandomBool()));
    }

    [Fact]
    public void Ctor_SetProperty()
    {
        var name = TestHelpers.GetRandom(GameArgumentNames.AllInternalSupportedArgumentNames);
        var isDebug = TestHelpers.RandomBool();
        var value = TestHelpers.RandomBool();

        var a = new TestFlagArg(name, value, TestHelpers.RandomBool(), isDebug);
        Assert.Equal(name, a.Name);
        Assert.Equal(value, a.Value);
        Assert.Equal(value, ((GameArgument)a).Value);
        Assert.Equal(isDebug, a.DebugArgument);
        Assert.True(a.IsValid(out _));
        Assert.False(a.HasPathValue);
    }

    [Fact]
    public void Ctor_TestIsDashed()
    {
        var flag = new TestFlagArg("Name", true);
        Assert.False(flag.Dashed);
        var dashed = new TestFlagArg("Name", true, true);
        Assert.True(dashed.Dashed);
    }

    [Fact]
    public void TestEquality()
    {
        var a1 = new TestFlagArg("Name", true);
        var a2 = new TestFlagArg("Name", false);
        var a3 = new TestFlagArg("Name", true, true);
        var a4 = new TestFlagArg("Name", true, false, true);
        var a5 = new TestFlagArg("Name2", true, false, true);
        var a6 = new TestFlagArg("Name", true);
        var a7 = new NamedOtherFlagArg("Name", true);

        Assert.False(a1.Equals(null));
        Assert.False(a1.Equals((object)null!));
        Assert.True(a1.Equals(a1));
        Assert.True(a1.Equals((object)a1));

        Assert.Equal<GameArgument>(a1, a6);
        Assert.NotEqual<GameArgument>(a1, a2);
        Assert.NotEqual<GameArgument>(a1, a3);
        Assert.Equal<GameArgument>(a1, a4);
        Assert.NotEqual<GameArgument>(a1, a5);
        Assert.NotEqual<GameArgument>(a1, a7);

        Assert.Equal<object>(a1, a6);
        Assert.NotEqual<object>(a1, a2);
        Assert.NotEqual<object>(a1, a3);
        Assert.Equal<object>(a1, a4);
        Assert.NotEqual<object>(a1, a5);
        Assert.NotEqual<object>(a1, a7);

        Assert.Equal(a1.GetHashCode(), a1.GetHashCode());
        Assert.Equal(a1.GetHashCode(), a6.GetHashCode());
        Assert.Equal(a1.GetHashCode(), a4.GetHashCode());

        Assert.NotEqual(a1.GetHashCode(), a2.GetHashCode());
        Assert.NotEqual(a1.GetHashCode(), a3.GetHashCode());
        Assert.NotEqual(a1.GetHashCode(), a5.GetHashCode());
    }

    [Fact]
    public void TestValueString()
    {
        var a1 = new TestFlagArg("Name", true);
        var a2 = new TestFlagArg("Name", false);
        var a3 = new TestFlagArg("Name", true, true);
        var a4 = new TestFlagArg("Name", false, true);

        Assert.Equal("True", a1.ValueToCommandLine());
        Assert.Equal("False", a2.ValueToCommandLine());
        Assert.Equal("True", a3.ValueToCommandLine());
        Assert.Equal("False", a4.ValueToCommandLine());
    }

    private class NamedOtherFlagArg(string name, bool value, bool dashed = false, bool debug = false)
        : FlagArgument(name, value, dashed, debug);
}