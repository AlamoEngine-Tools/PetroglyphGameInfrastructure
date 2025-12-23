using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using System;
using System.Collections.Generic;
using System.Linq;
using AET.Testing.Extensions;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing;

// ReSharper disable once InconsistentNaming
/// <summary>
/// Provides utility methods for testing game infrastructure components.
/// </summary>
public static class GITestUtilities
{
    private static readonly string[] PossibleLanguages = ["en", "de", "es", "it"];

    /// <summary>
    /// Gets a collection of real game platforms.
    /// </summary>
    public static ICollection<GamePlatform> RealPlatforms { get; } =
        [GamePlatform.Disk, GamePlatform.DiskGold, GamePlatform.SteamGold, GamePlatform.GoG, GamePlatform.Origin];

    /// <summary>
    /// Verifies that two <see cref="GameDetectionResult"/> instances are equal by comparing their properties.
    /// </summary>
    /// <param name="expected">The expected <see cref="GameDetectionResult"/> instance.</param>
    /// <param name="actual">The actual <see cref="GameDetectionResult"/> instance to compare against the expected one.</param>
    public static void AssertEqual(this GameDetectionResult expected, GameDetectionResult actual)
    {
        Assert.Equal(expected.Installed, actual.Installed);
        Assert.Equal(expected.GameIdentity, actual.GameIdentity);
        Assert.Equal(expected.GameLocation?.FullName, actual.GameLocation?.FullName);
        Assert.Equal(expected.InitializationRequired, actual.InitializationRequired);
    }

    /// <summary>
    /// Generates a random workshop flag for the specified game based on its platform.
    /// </summary>
    /// <param name="game">The game identity for which the workshop flag is being determined.</param>
    /// <returns>
    /// A random <see cref="bool"/> value, which is never <see langword="true"/> if <paramref name="game"/> does not support workshops.
    /// </returns>
    public static bool GetRandomWorkshopFlag(IGameIdentity game)
    {
        if (game.Platform is not GamePlatform.SteamGold)
            return false;
        return Random.Bool();
    }

    /// <summary>
    /// Retrieves a collection of real game platforms as enumerable test data.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of object arrays, where each array contains a single <see cref="GamePlatform"/>.
    /// </returns>
    public static IEnumerable<object[]> GetRealPlatforms()
    {
        return RealPlatforms.Select(platform => (object[])[platform]);
    }

    /// <summary>
    /// Generates a collection of real game identities for testing purposes.
    /// </summary>
    /// <remarks>
    /// This method yields game identities for all supported platforms and game types,
    /// including both Empire at War (Eaw) and Forces of Corruption (Foc).
    /// </remarks>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="object"/> arrays where each element contains
    /// a single <see cref="GameIdentity"/> instance representing a specific game type and platform.
    /// </returns>
    public static IEnumerable<object[]> RealGameIdentities()
    {
        foreach (var platform in RealPlatforms)
        {
            yield return [new GameIdentity(GameType.Eaw, platform)];
            yield return [new GameIdentity(GameType.Foc, platform)];
        }
    }

    /// <summary>
    /// Generates a collection of random languages with also randomized support levels.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ILanguageInfo"/> objects, where each object represents a language
    /// and its corresponding support level.
    /// </returns>
    public static ICollection<ILanguageInfo> GetRandomLanguages()
    {
        var languages = new HashSet<ILanguageInfo>();

        foreach (var _ in PossibleLanguages)
        {
            var code = Random.Item(PossibleLanguages);
            var support = Random.Enum<LanguageSupportLevel>();
            languages.Add(new LanguageInfo(code, support));
        }

        return languages;
    }

    /// <summary>
    /// Generates a random <see cref="IGameIdentity"/> instance.
    /// </summary>
    /// <param name="realOnly">
    /// If set to <see langword="true"/>, the method will only use platforms defined in <see cref="RealPlatforms"/>.
    /// If set to <see langword="true"/>, the method will include also the <see cref="GamePlatform.Undefined"/> platform.
    /// </param>
    /// <returns>
    /// A randomly generated <see cref="IGameIdentity"/> object, containing a random <see cref="GameType"/> 
    /// and a platform determined by the <paramref name="realOnly"/> parameter.
    /// </returns>
    public static IGameIdentity GetRandomGameIdentity(bool realOnly = true)
    {
        var platforms = realOnly ? RealPlatforms : (GamePlatform[])Enum.GetValues(typeof(GamePlatform));
        return new GameIdentity(Random.Enum<GameType>(), Random.Item(platforms));
    }

    /// <summary>
    /// Gets the opposite <see cref="GameType"/>.
    /// </summary>
    /// <param name="type">The current <see cref="GameType"/>.</param>
    /// <returns>
    /// The opposite <see cref="GameType"/>. If the input is <see cref="GameType.Eaw"/>, the result is <see cref="GameType.Foc"/>; 
    /// if the input is <see cref="GameType.Foc"/>, the result is <see cref="GameType.Eaw"/>.
    /// </returns>
    public static GameType Opposite(this GameType type)
    {
        return (GameType)((int)type ^ 1);
    }
}