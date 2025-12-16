using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using System;
using System.Collections.Generic;
using System.Linq;
using AET.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing;

// ReSharper disable once InconsistentNaming
public static class GITestUtilities
{
    public static ICollection<GamePlatform> RealPlatforms { get; } =
        [GamePlatform.Disk, GamePlatform.DiskGold, GamePlatform.SteamGold, GamePlatform.GoG, GamePlatform.Origin];

    private static readonly string[] PossibleLanguages = ["en", "de", "es", "it"];

    public static void AssertEqual(this GameDetectionResult expected, GameDetectionResult actual)
    {
        Assert.Equal(expected.Installed, actual.Installed);
        Assert.Equal(expected.GameIdentity, actual.GameIdentity);
        Assert.Equal(expected.GameLocation?.FullName, actual.GameLocation?.FullName);
        Assert.Equal(expected.InitializationRequired, actual.InitializationRequired);
    }

    public static bool GetRandomWorkshopFlag(IGame game)
    {
        if (game.Platform is not GamePlatform.SteamGold)
            return false;
        return new Random().NextDouble() >= 0.5;
    }

    public static IEnumerable<object[]> GetRealPlatforms()
    {
        return RealPlatforms.Select(platform => (object[])[platform]);
    }

    public static IEnumerable<object[]> RealGameIdentities()
    {
        foreach (var platform in RealPlatforms)
        {
            yield return [new GameIdentity(GameType.Eaw, platform)];
            yield return [new GameIdentity(GameType.Foc, platform)];
        }
    }

    public static ICollection<ILanguageInfo> GetRandomLanguages()
    {
        var languages = new HashSet<ILanguageInfo>();

        foreach (var _ in PossibleLanguages)
        {
            var code = TestHelpers.GetRandom(PossibleLanguages);
            var support = TestHelpers.GetRandomEnum<LanguageSupportLevel>();
            languages.Add(new LanguageInfo(code, support));
        }

        return languages;
    }
}