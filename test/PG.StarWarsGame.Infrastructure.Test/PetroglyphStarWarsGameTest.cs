using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using AET.Testing.Extensions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class PetroglyphStarWarsGameTest : PlayableModContainerTest
{
    private ITestingGameInstallation CreateGameInstallation(string? iconPath = null, ICollection<ILanguageInfo>? languages = null)
    {
        var gameId = new GameIdentity(Random.Enum<GameType>(), Random.Item(GITestUtilities.RealPlatforms));
        var gameInstallation = GetOrCreateGameInstallation(gameId);

        if (languages is not null)
        {
            foreach (var languageInfo in languages)
                gameInstallation.Game.InstallLanguage(languageInfo);
        }

        if (iconPath is not null)
        {
            iconPath = gameInstallation.Game.Type == GameType.Eaw ? "eaw.ico" : "foc.ico";
            FileSystem.File.Create(FileSystem.Path.Combine(gameInstallation.Game.Directory.FullName, iconPath));
        }

        return gameInstallation;
    }

    protected override ITestingPlayableObjectInstallation CreatePlayableObjectInstallation(
        string? iconPath = null, 
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreateGameInstallation(iconPath, languages);
    }

    protected override ITestingModContainerInstallation CreateModContainerInstallation()
    {
        return CreateGameInstallation();
    }

    [Fact]
    public void InvalidCtor_Throws()
    {
        var id = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
        Assert.Throws<ArgumentNullException>(() => 
            new PetroglyphStarWarsGame(null!, FileSystem.DirectoryInfo.New("path"), "name", ServiceProvider));
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(id, null!, "name", ServiceProvider));
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(id, FileSystem.DirectoryInfo.New("path"), null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() =>
            new PetroglyphStarWarsGame(id, FileSystem.DirectoryInfo.New("path"), "name", null!));
        Assert.Throws<ArgumentException>(() =>
            new PetroglyphStarWarsGame(id, FileSystem.DirectoryInfo.New("path"), "", ServiceProvider));
    }

    [Fact]
    public void Equals_GetHashCode()
    {
        var eawSteamId = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
        var focSteamId = new GameIdentity(GameType.Foc, GamePlatform.SteamGold);
        var eawDiskId = new GameIdentity(GameType.Eaw, GamePlatform.Disk);

        var eawSteam = GameInfrastructureTesting.Game(eawSteamId, ServiceProvider).Game;
        var focSteam = GameInfrastructureTesting.Game(focSteamId, ServiceProvider).Game;
        var eawDisc = GameInfrastructureTesting.Game(eawDiskId, ServiceProvider).Game;

        Assert.False(eawSteam.Equals(null));
        Assert.False(eawSteam.Equals((object)null!));
        Assert.False(eawSteam.Equals(new object()));

        Assert.True(eawSteam.Equals(eawSteam));
        Assert.True(eawSteam.Equals((object)eawSteam));
        Assert.True(((IGameIdentity)eawSteam).Equals(eawSteam));
        Assert.Equal(eawSteam.GetHashCode(), eawSteam.GetHashCode());

        Assert.False(eawSteam.Equals(focSteam));
        Assert.False(eawSteam.Equals((object)focSteam));
        Assert.False(eawSteam.Equals((IGameIdentity)focSteam));
        Assert.NotEqual(eawSteam.GetHashCode(), focSteam.GetHashCode());

        Assert.False(eawSteam.Equals(eawDisc));
        Assert.False(eawSteam.Equals((object)eawDisc));
        Assert.False(((IGameIdentity)eawSteam).Equals(eawDisc));
        Assert.NotEqual(eawSteam.GetHashCode(), eawDisc.GetHashCode());

        var otherLoc = FileSystem.DirectoryInfo.New("other/games/eaw");
        otherLoc.Create();
        var otherLocatedGame = new PetroglyphStarWarsGame(eawSteamId, otherLoc, "Other", ServiceProvider);
        
        Assert.False(eawSteam.Equals(otherLocatedGame));
        Assert.False(eawSteam.Equals((object)otherLocatedGame));
        Assert.True(((IGameIdentity)eawSteam).Equals(otherLocatedGame));
        Assert.NotEqual(eawSteam.GetHashCode(), otherLocatedGame.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void GetModDirTest(GameIdentity gameIdentity)
    {
        var game = GetOrCreateGameInstallation(gameIdentity).Game;
        var dataLocation = game.ModsLocation;
        Assert.Equal(FileSystem.Path.GetFullPath(FileSystem.Path.Combine(game.Directory.FullName, "Mods")), dataLocation.FullName);
    }
}