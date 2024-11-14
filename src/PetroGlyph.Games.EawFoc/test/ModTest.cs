using System;
using System.Collections.Generic;
using EawModinfo;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class ModTest : ModBaseTest
{
    private Mod CreatePhysicalMod(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        var game = CreateRandomGame();
        var mod = game.InstallMod("Mod", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        if (languages is not null)
        {
            foreach (var languageInfo in languages) 
                mod.InstallLanguage(languageInfo);
        }

        return mod;
    }

    protected override ModBase CreateMod(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreatePhysicalMod(iconPath, languages);
    }

    protected override IPlayableObject CreatePlayableObject(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreateMod(iconPath, languages);
    }

    protected override PlayableModContainer CreateModContainer()
    {
        return CreateMod();
    }

    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (IModinfo)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (string)null, null));
        var game = new Mock<IGame>();
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (IModinfo)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (string)null, null));
        var fs = new MockFileSystem();
        var modDir = fs.DirectoryInfo.New("Game/Mods/A");
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfo)null, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (string)null, new Mock<IServiceProvider>().Object));
        Assert.Throws<ArgumentException>(() => new Mod(game.Object, modDir, false, string.Empty, new Mock<IServiceProvider>().Object));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfo)null, sp.Object));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (string)null, sp.Object));

        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfo>().Object, null));
        Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, "name", null));

        Assert.Throws<ModinfoException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfo>().Object, sp.Object));
    }

    //[Fact]
    //public void ValidCtors_Properties()
    //{
    //    var modIdBuilder = new Mock<IModIdentifierBuilder>();
    //    var game = new Mock<IGame>();
    //    var fs = new MockFileSystem();
    //    var modDir = fs.DirectoryInfo.New("Game/Mods/A");
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(s => s.GetService(typeof(IModIdentifierBuilder))).Returns(modIdBuilder.Object);

    //    var mod = new Mod(game.Object, modDir, false, "Name", sp.Object);
    //    modIdBuilder.Setup(b => b.Build(mod)).Returns("somePath");

    //    Assert.Equal("Name", mod.Name);
    //    Assert.Equal(ModType.Default, mod.Type);
    //    Assert.Equal("somePath", mod.Identifier);
    //    Assert.Null(mod.ModInfo);

    //    var modA = new Mod(game.Object, modDir, true, "Name", sp.Object);
    //    Assert.Equal("somePath", modA.Identifier);
    //}
}