﻿using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal class SteamIdentifier(IServiceProvider serviceProvider) : SpecificPlatformIdentifier(serviceProvider)
{
    private static readonly string[] KnownSteamFiles =
    [
        "32470_install.vdf",
        "32472_install.vdf",
        "runm2.dat",
        "runme.dat",
        "runme.exe",
        "runme2.exe"
    ];

    public override bool IsPlatformFoc(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.MinimumGameFilesExist(GameType.Foc, location))
            return false;

        if (!ContainsSteamExe(location))
            return false;

        return location.Name.Equals("corruption", StringComparison.InvariantCultureIgnoreCase) &&
               ParentContainsSteamFiles(location);
    }

    public override bool IsPlatformEaw(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.MinimumGameFilesExist(GameType.Eaw, location))
            return false;

        if (!ContainsSteamExe(location))
            return false;

        return location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase) &&
               ParentContainsSteamFiles(location);
    }

    private static bool ParentContainsSteamFiles(IDirectoryInfo gameLocation)
    {
        var parentDir = gameLocation.Parent;
        return parentDir is not null && DirectoryContainsFiles(parentDir, KnownSteamFiles);
    }

    private static bool ContainsSteamExe(IFileSystemInfo directory)
    {
        const string exeFile = "StarWarsG.exe";
        var exePath = directory.FileSystem.Path.Combine(directory.FullName, exeFile);
        return directory.FileSystem.File.Exists(exePath);
    }
}