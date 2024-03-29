﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Steam.Test;

public class SteamGameLanguageFinderTest
{
    private readonly SteamGameLanguageFinder _service;
    private readonly Mock<ISteamWrapper> _steam;

    public SteamGameLanguageFinderTest()
    {
        var sc = new ServiceCollection();
        _steam = new Mock<ISteamWrapper>();
        sc.AddTransient(_ => _steam.Object);
        _service = new SteamGameLanguageFinder(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestSteamWrapperExists_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new SteamGameLanguageFinder(new ServiceCollection().BuildServiceProvider()));
    }

    [Fact]
    public void TestNotSteamGame_Throws()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
        Assert.Throws<InvalidOperationException>(() => _service.FindInstalledLanguages(game.Object));
    }

    [Fact]
    public void TestGameNotInstalled_Throws()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
        _steam.Setup(s => s.IsGameInstalled(32470u, out It.Ref<SteamAppManifest>.IsAny)).Returns(false);
        Assert.Throws<InvalidOperationException>(() => _service.FindInstalledLanguages(game.Object));
    }

    [Fact]
    public void TestNoOtherInstalledLanguages()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);

        var fs = new MockFileSystem();

        var manifest = new SteamAppManifest(new Mock<ISteamLibrary>().Object, new MockFileInfo(fs, "test.path"), 123,
            "name", new MockDirectoryInfo(fs, "path"), SteamAppState.StateFullyInstalled, new HashSet<uint>());

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

        var manifest = new SteamAppManifest(new Mock<ISteamLibrary>().Object, new MockFileInfo(fs, "test.path"), 123,
            "name", new MockDirectoryInfo(fs, "path"), SteamAppState.StateFullyInstalled, new HashSet<uint>
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