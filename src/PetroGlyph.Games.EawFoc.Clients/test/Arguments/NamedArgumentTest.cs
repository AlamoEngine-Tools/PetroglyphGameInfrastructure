using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class NamedArgumentTest
{
    [Fact]
    public void TestEquality()
    {
        var a1 = new ArgA("Name", "value", false);
        var a2 = new ArgA("Name", "Value", false);
        var a3 = new ArgA("Name1", "value", false);
        var a4 = new ArgA("Name", "value", true);
        var a5 = new ArgA("Name", "value", false);

        var b = new ArgB("Name", "value", false);
        var c = new ArgC("Name", 0, false);

        Assert.Equal<IGameArgument>(a1, a4);
        Assert.Equal<IGameArgument>(a1, a5);
        Assert.Equal<IGameArgument>(a1, b);
        Assert.NotEqual<IGameArgument>(a1, a2);
        Assert.NotEqual<IGameArgument>(a1, a3);
        Assert.NotEqual<IGameArgument>(a1, c);
    }

    private class ArgA : NamedArgument<string>
    {
        public ArgA(string name, string value, bool isDebug) : base(name, value, isDebug)
        {
        }

        public override string ValueToCommandLine()
        {
            return "";
        }
    }

    private class ArgB : NamedArgument<string>
    {
        public ArgB(string name, string value, bool isDebug) : base(name, value, isDebug)
        {
        }

        public override string ValueToCommandLine()
        {
            return "";
        }
    }

    private class ArgC : NamedArgument<int>
    {
        public ArgC(string name, int value, bool isDebug) : base(name, value, isDebug)
        {
        }

        public override string ValueToCommandLine()
        {
            return "";
        }
    }
}