﻿using System;
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

        return game.Directory
            .EnumerateFiles(exeFileName!, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
    }

    private static string? GetExecutableFileName(IGame game, GameBuildType buildType)
    {
        if (game.Platform == GamePlatform.SteamGold)
            return GetSteamFileName(buildType);

        if (buildType is GameBuildType.Debug)
            return null;

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