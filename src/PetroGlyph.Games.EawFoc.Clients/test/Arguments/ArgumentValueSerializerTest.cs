using System;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Sklavenwalker.CommonUtilities.FileSystem;
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

    [Fact]
    public void TestShortenCallsCorrectMethods()
    {
        var helper = new Mock<IPathHelperService>();
        var serializer = new ArgumentValueSerializer(helper.Object);

        var fs = new MockFileSystem();
        var game = fs.DirectoryInfo.FromDirectoryName("game");
        var mod = fs.DirectoryInfo.FromDirectoryName("game/mod");

        serializer.ShortenPath(mod, game);

        helper.Verify(h => h.GetRelativePath(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
        helper.Verify(h => h.NormalizePath(It.IsAny<string>(), PathNormalizeOptions.FullNoResolve), Times.Exactly(1));
    }

    [Fact]
    public void TestShorten_Integration()
    {
        var serializer = new ArgumentValueSerializer();

        var fs = new MockFileSystem();
        var game = fs.DirectoryInfo.FromDirectoryName("game");
        var mod = fs.DirectoryInfo.FromDirectoryName("game/mod");

        serializer.ShortenPath(mod, game);

        Assert.Equal("mod", serializer.ShortenPath(mod, game));
    }
}