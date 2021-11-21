using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

public class FlagArgumentTest
{
    [Fact]
    public void TestEquality()
    {
        var a1 = new Flag("Name", true);
        var a2 = new Flag("Name", false);
        var a3 = new Flag("Name", true, true);
        var a4 = new Flag("Name", true, false, true);
        var a5 = new Flag("Name2", true, false, true);
        var a6 = new Flag("Name", true);

        Assert.Equal<IGameArgument>(a1, a6);
        Assert.NotEqual<IGameArgument>(a1, a2);
        Assert.NotEqual<IGameArgument>(a1, a3);
        Assert.Equal<IGameArgument>(a1, a4);
        Assert.NotEqual<IGameArgument>(a1, a5);
    }

    [Fact]
    public void TestValueString()
    {
        var a1 = new Flag("Name", true);
        var a2 = new Flag("Name", false);
        var a3 = new Flag("Name", true, true);
        var a4 = new Flag("Name", false, true);

        Assert.Equal("Name", a1.ValueToCommandLine());
        Assert.Equal(string.Empty, a2.ValueToCommandLine());
        Assert.Equal("Name", a3.ValueToCommandLine());
        Assert.Equal(string.Empty, a4.ValueToCommandLine());
    }

    private class Flag : FlagArgument
    {
        public Flag(string name, bool value, bool dashed = false, bool debug = false) : base(name, value, dashed, debug)
        {
        }
    }
}