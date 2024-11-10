using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class PetroglyphStarWarsGameTest : PlayableModContainerTest
{
    private PetroglyphStarWarsGame CreateGame()
    {
        var gameId = new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), TestHelpers.GetRandom(GITestUtilities.RealPlatforms));
        return FileSystem.InstallGame(gameId, ServiceProvider);
    }

    protected override IPlayableObject CreatePlayableObject()
    {
        return CreateGame();
    }

    protected override PlayableModContainer CreateModContainer()
    {
        return CreateGame();
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

        var eawSteam = FileSystem.InstallGame(eawSteamId, ServiceProvider);
        var focSteam = FileSystem.InstallGame(focSteamId, ServiceProvider);
        var eawDisc = FileSystem.InstallGame(eawDiskId, ServiceProvider);

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
    [MemberData(nameof(RealGameIdentities))]
    public void GetModDirTest(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        var dataLocation = game.ModsLocation;
        Assert.Equal(FileSystem.Path.GetFullPath(FileSystem.Path.Combine(game.Directory.FullName, "Mods")), dataLocation.FullName);
    }
}