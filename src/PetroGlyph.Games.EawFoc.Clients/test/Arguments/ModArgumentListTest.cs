using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ModArgumentListTest
{
    [Fact]
    public void TestProps()
    {
        var arg = new ModArgumentList(new List<ModArgument> { new("path", false) });
        Assert.Equal(GameArgumentNames.ModListArg, arg.Name);
        Assert.Equal(ArgumentKind.ModList, arg.Kind);
        Assert.Empty(arg.ValueToCommandLine());
        Assert.Single(arg.Value);
        var v = Assert.IsAssignableFrom<IReadOnlyList<ModArgument>>(((GameArgument)arg).Value);
        Assert.Single(v);
        Assert.False(arg.HasPathValue);
    }

    [Fact]
    public void TestEmpty()
    {
        var arg = ModArgumentList.Empty;
        Assert.Empty(arg.Value);
        Assert.True(arg.IsValid(out _));
        Assert.Empty(arg.ValueToCommandLine());
        var v = Assert.IsAssignableFrom<IReadOnlyList<ModArgument>>(((GameArgument)arg).Value);
        Assert.Empty(v);
    }

    [Fact]
    public void TestEquals()
    {
        var a = ModArgumentList.Empty;
        
        Assert.False(a.Equals(null));
        Assert.False(a.Equals((object)null!));

        Assert.True(a.Equals(a));
        Assert.True(a.Equals((object)a));
        Assert.Equal(a.GetHashCode(), a.GetHashCode());

        var b = new ModArgumentList(new List<ModArgument>());
        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());

        var c = new ModArgumentList(new List<ModArgument> { new("path", false) });
        Assert.False(a.Equals(c));
        Assert.False(a.Equals((object)c));
        Assert.NotEqual(a.GetHashCode(), c.GetHashCode());

        var d = new ModArgumentList(new List<ModArgument> { new("path", true) });
        Assert.False(c.Equals(d));
        Assert.False(c.Equals((object)d));
        Assert.NotEqual(c.GetHashCode(), d.GetHashCode());

        var e = new ModArgumentList(new List<ModArgument> { new("path", true) });
        Assert.True(d.Equals(e));
        Assert.True(d.Equals((object)e));
        Assert.Equal(d.GetHashCode(), e.GetHashCode());

        var i = new InvalidModListArg();
        Assert.False(a.Equals(i));
        Assert.False(a.Equals((object)i));
    }
}