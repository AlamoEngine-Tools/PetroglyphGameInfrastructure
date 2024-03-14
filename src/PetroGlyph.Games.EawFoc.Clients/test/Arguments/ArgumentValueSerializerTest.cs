using System;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

public class ArgumentValueSerializerTest
{
    [Fact]
    public void TestSerializeFP()
    {
        var serializer = new ArgumentValueSerializer();
        Assert.Equal("1.1", serializer.Serialize(1.1f));
        Assert.Equal("1.1", serializer.Serialize(1.1d));
    }

    [Fact]
    public void TestSerializeUnsigned()
    {
        var serializer = new ArgumentValueSerializer();
        Assert.Equal("1", serializer.Serialize(1u));
    }

    [Fact]
    public void TestBool()
    {
        var serializer = new ArgumentValueSerializer();
        Assert.Equal("True", serializer.Serialize(true));
        Assert.Equal("False", serializer.Serialize(false));
    }

    [Fact]
    public void TestSerializeSignedNotSupported()
    {
        var serializer = new ArgumentValueSerializer();
        Assert.Throws<InvalidOperationException>(() => serializer.Serialize(-1));
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void TestShorten_Integration_Linux()
    {
        var serializer = new ArgumentValueSerializer();

        var fs = new MockFileSystem();
        var game = fs.DirectoryInfo.New("game");
        var mod = fs.DirectoryInfo.New("game/mod");

        serializer.ShortenPath(mod, game);

        Assert.Equal("mod", serializer.ShortenPath(mod, game));
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void TestShorten_Integration_Windows()
    {
        var serializer = new ArgumentValueSerializer();

        var fs = new MockFileSystem();
        var game = fs.DirectoryInfo.New("game");
        var mod = fs.DirectoryInfo.New("game/mod");

        serializer.ShortenPath(mod, game);

        Assert.Equal("MOD", serializer.ShortenPath(mod, game));
    }
}