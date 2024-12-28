using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients.Utilities;

internal class GameExecutableFileUtilities
{
    private const string SteamFileNameBase = "StarWars";
    private const string SteamReleaseSuffix = "G";
    private const string SteamDebugSuffix = "I";

    public static IFileInfo? GetExecutableForGame(IGame game, GameBuildType buildType)
    {
        var exeFileName = GetExecutableFileName(game, buildType);
        if (string.IsNullOrEmpty(exeFileName))
            return null;

#if NETSTANDARD2_1_OR_GREATER || NET
        return game.Directory
            .EnumerateFiles(exeFileName, new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive })
            .FirstOrDefault();
#else
        return game.Directory
            .EnumerateFiles(exeFileName, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
#endif

    }

    public static string GetExecutableFileName(IGame game, GameBuildType buildType)
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
        var suffix = buildType switch
        {
            GameBuildType.Release => SteamReleaseSuffix,
            GameBuildType.Debug => SteamDebugSuffix,
            _ => throw new ArgumentOutOfRangeException(nameof(buildType), buildType, null)
        };
        return $"{SteamFileNameBase}{suffix}.exe";
    }
}