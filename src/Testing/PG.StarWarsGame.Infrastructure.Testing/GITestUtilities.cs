using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Testing;

public static class GITestUtilities
{
    public static ICollection<GamePlatform> RealPlatforms { get; } =
        [GamePlatform.Disk, GamePlatform.DiskGold, GamePlatform.SteamGold, GamePlatform.GoG, GamePlatform.Origin];

    public static void AssertEqual(this GameDetectionResult expected, GameDetectionResult actual)
    {
        Assert.Equal(expected.GameIdentity, actual.GameIdentity);
        Assert.Equal(expected.GameLocation?.FullName, actual.GameLocation?.FullName);
        Assert.Equal(expected.InitializationRequired, actual.InitializationRequired);
        Assert.Equal(expected.Installed, actual.Installed);
    }

    public static bool GetRandomWorkshopFlag(IGame game)
    {
        if (game.Platform is not GamePlatform.SteamGold)
            return false;
        return new Random().NextDouble() >= 0.5;
    }
}