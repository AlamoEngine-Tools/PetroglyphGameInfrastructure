using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal class OriginIdentifier(IServiceProvider serviceProvider) : SpecificPlatformIdentifier(serviceProvider)
{
    private static readonly HashSet<string> KnownOriginDirs =
    [
        "Manuals",
        "__Installer"
    ];

    public override bool IsPlatformFoc(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.GameExeExists(location, GameType.Foc))
        {
            Logger?.LogTrace("Unable to find FoC Origin at first location. Trying to fix broken registry path");
            TryFixBrokenFocLocation(ref location);
            if (!GameDetectorBase.GameExeExists(location, GameType.Foc))
            {
                Logger?.LogTrace("Origin location fix was unsuccessful. This is not a Origin installation.");
                return false;
            }
        }

        if (!GameDetectorBase.DataAndMegaFilesXmlExists(location))
            return false;

        if (!DirectoryContainsFiles(location, ["EALaunchHelper.exe"]))
            return false;

        return ParentContainsOriginDirectories(location);
    }

    public override bool IsPlatformEaw(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.MinimumGameFilesExist(GameType.Eaw, location))
        {
            Logger?.LogTrace("Unable to find EaW Origin at first location.");
            return false;
        }

        // TODO: Do we have EALaunchHelper.exe here too?

        return location.Name.Equals("GameData", StringComparison.OrdinalIgnoreCase) &&
               ParentContainsOriginDirectories(location);
    }

    private void TryFixBrokenFocLocation(ref IDirectoryInfo location)
    {
        if (location.Name.Equals("EAWX", StringComparison.OrdinalIgnoreCase))
            return;
        if (!location.Name.Equals("corruption", StringComparison.OrdinalIgnoreCase))
        {
            Logger?.LogTrace("Unable to apply Origin fix to a directory called other than 'corruption'.");
            return;
        }

        Logger?.LogTrace("Changing directory name from 'corruption' to 'EAWX'");
        var parentDir = location.Parent!;

        var correctedPath = location.FileSystem.Path.Combine(parentDir.FullName, "EAWX");
        if (!location.FileSystem.Directory.Exists(correctedPath))
        {
            Logger?.LogTrace($"Unable to sanitize origin path: Corrected path '{correctedPath}' does not exists.");
            return;
        }
        location = location.FileSystem.DirectoryInfo.New(correctedPath);
    }

    private static bool ParentContainsOriginDirectories(IDirectoryInfo gameLocation)
    {
        var parentDir = gameLocation.Parent;
        return parentDir is not null && DirectoryContainsFolders(parentDir, KnownOriginDirs);
    }
}