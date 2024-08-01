using System;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class ModTest
{
    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (IModinfo)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (IModinfoFile)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (string)null, null));
        var game = new Mock<IGame>();
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (IModinfo)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (IModinfoFile)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (string)null, null));
        var fs = new MockFileSystem();
        var modDir = fs.DirectoryInfo.New("Game/Mods/A");
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfo)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfoFile)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (string)null, new Mock<IServiceProvider>().Object));
        Assert.Throws<ArgumentException>(() => new Mod(game.Object, modDir, false, string.Empty, new Mock<IServiceProvider>().Object));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfo)null, sp.Object));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfoFile)null, sp.Object));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (string)null, sp.Object));

        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfo>().Object, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfoFile>().Object, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, "name", null));

        Assert.Throws<ModinfoException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfo>().Object, sp.Object));
    }

    [Fact]
    public void ValidCtors_Properties()
    {
        var modIdBuilder = new Mock<IModIdentifierBuilder>();
        var game = new Mock<IGame>();
        var fs = new MockFileSystem();
        var modDir = fs.DirectoryInfo.New("Game/Mods/A");
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(IModIdentifierBuilder))).Returns(modIdBuilder.Object);

        var mod = new Mod(game.Object, modDir, false, "Name", sp.Object);
        modIdBuilder.Setup(b => b.Build(mod)).Returns("somePath");

        Assert.Equal("Name", mod.Name);
        Assert.Equal(ModType.Default, mod.Type);
        Assert.Equal("somePath", mod.Identifier);
        Assert.NotNull(mod.FileSystem);
        Assert.Null(mod.ModinfoFile);
        Assert.Null(mod.ModInfo);

        var file = new Mock<IModinfoFile>();
        file.Setup(f => f.GetModinfo()).Returns(new ModinfoData("Name"));
        var modA = new Mod(game.Object, modDir, false, file.Object, sp.Object);
        Assert.NotNull(modA.ModinfoFile);

        var modB = new Mod(game.Object, modDir, true, "Name", sp.Object);
        Assert.Equal("somePath", modB.Identifier);
    }
}