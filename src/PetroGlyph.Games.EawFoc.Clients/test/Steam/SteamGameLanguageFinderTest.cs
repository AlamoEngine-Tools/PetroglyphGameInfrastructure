using System;
using System.Collections.Generic;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Language;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Steam;

public class SteamGameLanguageFinderTest
{
    private readonly SteamGameLanguageFinder _service;
    private readonly Mock<ISteamWrapper> _steam = new();
    private readonly Mock<ILanguageFinder> _languageFinder = new();

    public SteamGameLanguageFinderTest()
    {
        var sc = new ServiceCollection();
        sc.AddTransient(_ => _steam.Object);
        sc.AddTransient(_ => _languageFinder.Object);
        _service = new SteamGameLanguageFinder(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestSteamWrapperExists_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new SteamGameLanguageFinder(new ServiceCollection().BuildServiceProvider()));
    }

    [Fact]
    public void TestNotSteamGame_Fallback()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        _languageFinder.Setup(f => f.Merge(It.IsAny<IEnumerable<ILanguageInfo>[]>())).Returns(new HashSet<ILanguageInfo>{new LanguageInfo("de", LanguageSupportLevel.SFX)});

        var result = _service.FindInstalledLanguages(game.Object);
        Assert.Equivalent(new HashSet<ILanguageInfo>{new LanguageInfo("de", LanguageSupportLevel.SFX)},result);
    }

    [Fact]
    public void TestGameNotInstalled_Fallback()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        _steam.Setup(s => s.IsGameInstalled(32470u, out It.Ref<SteamAppManifest>.IsAny!)).Returns(false);

        _languageFinder.Setup(f => f.Merge(It.IsAny<IEnumerable<ILanguageInfo>[]>())).Returns(new HashSet<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) });

        var result = _service.FindInstalledLanguages(game.Object);
        Assert.Equivalent(new HashSet<ILanguageInfo> { new LanguageInfo("de", LanguageSupportLevel.SFX) }, result);
    }

    [Fact]
    public void TestNoOtherInstalledLanguages()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);

        var fs = new MockFileSystem();

        var manifest = new SteamAppManifest(new Mock<ISteamLibrary>().Object, fs.FileInfo.New("test.path"), 123,
            "name", fs.DirectoryInfo.New("path"), SteamAppState.StateFullyInstalled, new HashSet<uint>());

        _steam.Setup(s => s.IsGameInstalled(32470u, out manifest)).Returns(true);

        var languages = _service.FindInstalledLanguages(game.Object);
        var english = Assert.Single(languages);
        Assert.Equal(new LanguageInfo("en", LanguageSupportLevel.FullLocalized), english);
    }

    [Fact]
    public void TestOtherInstalledLanguages()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);

        var fs = new MockFileSystem();

        var manifest = new SteamAppManifest(new Mock<ISteamLibrary>().Object, fs.FileInfo.New("test.path"), 123,
            "name", fs.DirectoryInfo.New("path"), SteamAppState.StateFullyInstalled, new HashSet<uint>
            {
                32473, 32474, 32475, 32476
            });

        _steam.Setup(s => s.IsGameInstalled(32470u, out manifest)).Returns(true);

        var languages = _service.FindInstalledLanguages(game.Object);
        Assert.Equal(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("fr", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("de", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("it", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("es", LanguageSupportLevel.FullLocalized)
        }, languages);
    }
}