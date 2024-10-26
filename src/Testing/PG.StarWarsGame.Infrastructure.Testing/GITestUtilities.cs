using System;
using System.Collections.Generic;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Testing;

public static class GITestUtilities
{
    public static IEnumerable<GamePlatform> EnumerateRealPlatforms()
    {
        foreach (GamePlatform platform in Enum.GetValues(typeof(GamePlatform)))
        {
            if (platform is not GamePlatform.Undefined) 
                yield return platform;
        }
    }

    public static void AssertEqual(this GameDetectionResult expected, GameDetectionResult actual)
    {
        Assert.Equal(expected.GameIdentity, actual.GameIdentity);
        Assert.Equal(expected.GameLocation?.FullName, actual.GameLocation?.FullName);
        Assert.Equal(expected.InitializationRequired, actual.InitializationRequired);
        Assert.Equal(expected.Installed, actual.Installed);
    }
}