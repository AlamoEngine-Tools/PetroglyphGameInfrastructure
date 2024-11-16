using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
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

        if (iconPath is not null) 
            FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, iconPath));

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
        var game = CreateRandomGame();
        Assert.Throws<ArgumentNullException>(() => new Mod(null!, FileSystem.DirectoryInfo.New("modPath"), false, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new Mod(game, null!, false, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new Mod(game, FileSystem.DirectoryInfo.New("modPath"), false, (IModinfo)null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new Mod(game, FileSystem.DirectoryInfo.New("modPath"), false, (string)null!, ServiceProvider));
        Assert.Throws<ArgumentException>(() => new Mod(game, FileSystem.DirectoryInfo.New("modPath"), false, "", ServiceProvider));
    }

    [Fact]
    public void ValidCtors_Properties_FromName()
    {
        var game = CreateRandomGame();
        var ws = GITestUtilities.GetRandomWorkshopFlag(game);

        var mod = game.InstallAndAddMod("Mod", ws, ServiceProvider);

        Assert.Equal("Mod", mod.Name);
        Assert.Equal(ws ? ModType.Workshops : ModType.Default, mod.Type);

        if (ws)
            Assert.True(uint.TryParse(mod.Identifier, out _));
        else
            Assert.Equal(mod.Directory.FullName.ToUpperInvariant(), mod.Identifier);

        Assert.Null(mod.ModInfo);
    }

    [Fact]
    public void ValidCtors_Properties_FromModinfo()
    {
        var game = CreateRandomGame();
        var ws = GITestUtilities.GetRandomWorkshopFlag(game);

        var modInfo = new ModinfoData("Mod");
        var mod = game.InstallAndAddMod(ws, modInfo,ServiceProvider);

        Assert.Equal("Mod", mod.Name);
        Assert.Equal(ws ? ModType.Workshops : ModType.Default, mod.Type);

        if (ws)
            Assert.True(uint.TryParse(mod.Identifier, out _));
        else
            Assert.Equal(mod.Directory.FullName.ToUpperInvariant(), mod.Identifier);

        Assert.Same(modInfo, mod.ModInfo);
    }
}