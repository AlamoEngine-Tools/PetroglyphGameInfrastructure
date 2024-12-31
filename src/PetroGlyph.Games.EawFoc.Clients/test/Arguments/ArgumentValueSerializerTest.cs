using System;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class ArgumentValueSerializerTest
{
    [Fact]
    public void Serialize_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ArgumentValueSerializer.Serialize(null!));
    }

    [Theory]
    [InlineData("This is some string")]
    [InlineData("AlsoSomeString")]
    [InlineData("")]
    public void Serialize_String(string input)
    {
        Assert.Equal(input, ArgumentValueSerializer.Serialize(input));
    }

    [Fact]
    public void Serialize_FP()
    {
        Assert.Equal("1.1", ArgumentValueSerializer.Serialize(1.1f));
        Assert.Equal("1.1", ArgumentValueSerializer.Serialize(1.1d));
    }

    [Fact]
    public void Serialize_Uint()
    {
        Assert.Equal("1", ArgumentValueSerializer.Serialize(1u));
    }

    [Fact]
    public void Serialize_Bool()
    {
        Assert.Equal("True", ArgumentValueSerializer.Serialize(true));
        Assert.Equal("False", ArgumentValueSerializer.Serialize(false));
    }

    [Theory]
    [InlineData(AILogStyle.Heavy, "Heavy")]
    [InlineData(AILogStyle.Normal, "Normal")]
    public void Serialize_EnumByName(AILogStyle value, string expected)
    {
        Assert.Equal(expected, ArgumentValueSerializer.Serialize(value));
    }

    [Theory]
    [InlineData(MyEnumSerializeByValue.A, 1)]
    [InlineData(MyEnumSerializeByValue.B, 2)]
    public void Serialize_EnumByValue(MyEnumSerializeByValue value, int expected)
    {
        Assert.Equal(expected.ToString(), ArgumentValueSerializer.Serialize(value));
    }

    [Fact]
    public void Serialize_UnknownEnum_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => ArgumentValueSerializer.Serialize(SomeRandomEnum.Value));
    }

    [Fact]
    public void Serialize_NotSupportedType()
    {
        Assert.Throws<InvalidOperationException>(() => ArgumentValueSerializer.Serialize(-1));
    }

    [PlatformSpecificTheory(TestPlatformIdentifier.Linux)]
    [InlineData("game/mod/my", "game", "mod\\my")]
    [InlineData("game/mod", "game", "mod")]
    [InlineData("game/mod", "GAME", "game/mod", true)]
    [InlineData("mod", "game", "mod", true)]
    [InlineData("with space/other", "with space/game", "../other", false)]
    public void TestShorten_Integration_Linux(string targetPath, string basePath, string expected, bool makeExpectedFullPath = false)
    {
         var fs = new MockFileSystem();
        var game = fs.DirectoryInfo.New(basePath);
        var mod = fs.DirectoryInfo.New(targetPath);

        if (makeExpectedFullPath)
            expected = fs.Path.GetFullPath(expected);

        Assert.Equal(expected, ArgumentValueSerializer.ShortenPath(mod, game));
    }

    [PlatformSpecificTheory(TestPlatformIdentifier.Windows)]
    [InlineData("game/mod/my", "game", "MOD\\MY")]
    [InlineData("game/mod", "GAME", "MOD")]
    [InlineData("mod", "game", "MOD", true)]
    [InlineData("with space/other", "with space/game", "..\\OTHER", false)]
    public void TestShorten_Integration_Windows(string targetPath, string basePath, string expected, bool makeExpectedFullPath = false)
    {
        var fs = new MockFileSystem();
        var game = fs.DirectoryInfo.New(basePath);
        var mod = fs.DirectoryInfo.New(targetPath);

        if (makeExpectedFullPath)
            expected = fs.Path.GetFullPath(expected).ToUpperInvariant();

        Assert.Equal(expected, ArgumentValueSerializer.ShortenPath(mod, game));
    }
}