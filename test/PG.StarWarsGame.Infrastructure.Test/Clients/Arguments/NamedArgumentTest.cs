using System;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class NamedArgumentTest : GameArgumentTestBase
{
    [Fact]
    public void Ctor_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TestNamedArg(null!, "value", false));
        Assert.Throws<ArgumentException>(() => new TestNamedArg(string.Empty, "value", false));
        Assert.Throws<ArgumentNullException>(() => new TestNamedArg("value", null!, false));
        Assert.Throws<ArgumentNullException>(() => new NamedArgNullable("value", null, false));
    }

    [Fact]
    public void Ctor_SetProperty()
    {
        var name = TestHelpers.GetRandom(GameArgumentNames.AllInternalSupportedArgumentNames);
        var isDebug = TestHelpers.RandomBool();
        var a = new TestNamedArg(name, "value", isDebug);

        Assert.Equal(name, a.Name);
        Assert.Equal("value", a.Value);
        Assert.Equal("value", ((GameArgument)a).Value);
        Assert.Equal("value", a.ValueToCommandLine());
        Assert.Equal(isDebug, a.DebugArgument);
        Assert.True(a.IsValid(out _));
    }

    [Theory]
    [InlineData("game/mod/my", "game", "mod/my")]
    [InlineData("mod", "game", "mod", true)]
    [InlineData("with space/other", "with space/game", "../other", false)]
    public void ValueToCommandLine_ShortenPath(string targetPath, string basePath, string expected, bool makeExpectedFullPath = false)
    {
        var fs = new MockFileSystem();

        if (makeExpectedFullPath)
            expected = fs.Path.GetFullPath(expected);

        expected = PathNormalizer.Normalize(expected, new PathNormalizeOptions()
        {
            UnifyCase = UnifyCasingKind.UpperCase,
            TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim,
            UnifyDirectorySeparators = true
        });

        foreach (var pathArg in GetPathKeyValueArgs(targetPath, basePath, fs)) 
            Assert.Equal(expected, pathArg.ValueToCommandLine());
    }

    [Fact]
    public void TestEquality()
    {
        var a1 = new TestNamedArg("Name", "value", false);
        var a2 = new TestNamedArg("Name", "Value", false);
        var a3 = new TestNamedArg("Name1", "value", false);
        var a4 = new TestNamedArg("Name", "value", true);
        var a5 = new TestNamedArg("Name", "value", false);

        var b = new NamedArgB("Name", "value", false);
        var c = new NamedArgC("Name", 0, false);

        Assert.False(a1.Equals(null));
        Assert.False(a1.Equals((object)null!));
        Assert.True(a1.Equals(a1));
        Assert.True(a1.Equals((object)a1));

        Assert.Equal<GameArgument>(a1, a4);
        Assert.Equal<GameArgument>(a1, a5);
        Assert.Equal<object>(a1, a4);
        Assert.Equal<object>(a1, a5);
        Assert.Equal(a1.GetHashCode(), a4.GetHashCode());
        Assert.Equal(a1.GetHashCode(), a5.GetHashCode());
       

        Assert.NotEqual<GameArgument>(a1, a2);
        Assert.NotEqual<GameArgument>(a1, b);
        Assert.NotEqual<GameArgument>(a1, a3);
        Assert.NotEqual<GameArgument>(a1, c);
        Assert.NotEqual<object>(a1, a2);
        Assert.NotEqual<object>(a1, b);
        Assert.NotEqual<object>(a1, a3);
        Assert.NotEqual<object>(a1, c);
        Assert.NotEqual(a1.GetHashCode(), a2.GetHashCode());
        Assert.NotEqual(a1.GetHashCode(), a3.GetHashCode());
        Assert.NotEqual(a1.GetHashCode(), c.GetHashCode());
    }

    private class NamedArgB(string name, string value, bool isDebug) : NamedArgument<string>(name, value, isDebug);
    private class NamedArgC(string name, uint value, bool isDebug) : NamedArgument<uint>(name, value, isDebug);
    private class NamedArgNullable(string name, uint? value, bool isDebug) : NamedArgument<uint?>(name, value, isDebug);

}