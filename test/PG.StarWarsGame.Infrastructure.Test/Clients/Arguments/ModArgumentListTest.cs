using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class ModArgumentListTest
{
    [Fact]
    public void TestProps()
    {
        var fs = new MockFileSystem();
        var gameDir = fs.DirectoryInfo.New("game");
        var modDir = fs.DirectoryInfo.New("game/mods/myMod");

        var arg = new ModArgumentList(new List<ModArgument> { new(modDir, gameDir, false) });
        Assert.Equal(GameArgumentNames.ModListArg, arg.Name);
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
        var fs = new MockFileSystem();
        var gameDir = fs.DirectoryInfo.New("game");
        var modDir = fs.DirectoryInfo.New("game/mods/myMod");

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

        var c = new ModArgumentList(new List<ModArgument> { new(modDir, gameDir, false) });
        Assert.False(a.Equals(c));
        Assert.False(a.Equals((object)c));
        Assert.NotEqual(a.GetHashCode(), c.GetHashCode());

        var d = new ModArgumentList(new List<ModArgument> { new(modDir, gameDir, true) });
        Assert.False(c.Equals(d));
        Assert.False(c.Equals((object)d));
        Assert.NotEqual(c.GetHashCode(), d.GetHashCode());

        var e = new ModArgumentList(new List<ModArgument> { new(modDir, gameDir, true) });
        Assert.True(d.Equals(e));
        Assert.True(d.Equals((object)e));
        Assert.Equal(d.GetHashCode(), e.GetHashCode());
    }
}