using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

internal sealed class GameExecutableNameBuilder : IGameExecutableNameBuilder
{
    private const string SteamFileNameBase = "StarWars";
    private const string SteamReleaseSuffix = "G";
    private const string SteamDebugSuffix = "I";

    public string GetExecutableFileName(IGame game, GameBuildType buildType)
    {
        if (game.Platform == GamePlatform.SteamGold)
            return GetSteamFileName(buildType);
        return game.Type switch
        {
            GameType.EaW => PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName,
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