using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients.Utilities;

/// <summary>
/// Provides utility methods for locating game executable files for different game platforms and build types.
/// </summary>
public static class GameExecutableFileUtilities
{
    private const string SteamFileNameBase = "StarWars";
    private const string SteamReleaseSuffix = "G";
    private const string SteamDebugSuffix = "I";

    /// <summary>
    /// Gets the executable file for the specified game and build type or <see langword="null"/> if not found.
    /// </summary>
    /// <param name="game">The game for which to locate the executable file.</param>
    /// <param name="buildType">The build type of the game executable to locate.</param>
    /// <returns>An <see cref="IFileInfo"/> representing the executable file if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    public static IFileInfo? GetExecutableForGame(IGame game, GameBuildType buildType)
    {
        if (game == null) 
            throw new ArgumentNullException(nameof(game));

        // Only SteamGold supports debug builds
        if (buildType == GameBuildType.Debug && game.Platform != GamePlatform.SteamGold)
            return null;

        var exeFileName = GetExecutableFileName(game, buildType);

        return game.Directory
            .EnumerateFiles(exeFileName, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
    }

    private static string GetExecutableFileName(IGame game, GameBuildType buildType)
    {
        if (game.Platform == GamePlatform.SteamGold)
            return GetSteamFileName(buildType);

        return game.Type switch
        {
            GameType.Eaw => PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName,
            GameType.Foc => PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetSteamFileName(GameBuildType buildType)
    {
        var suffix = SteamReleaseSuffix;
        if (buildType == GameBuildType.Debug)
            suffix = SteamDebugSuffix;
        return $"{SteamFileNameBase}{suffix}.exe";
    }
}