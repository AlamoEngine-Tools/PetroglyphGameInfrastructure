using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal class GogIdentifier(IServiceProvider serviceProvider) : SpecificPlatformIdentifier(serviceProvider)
{
    private static readonly string[] KnownGogFiles =
    [
        "Language.exe",
        "goggame.sdb",
        "goggame-1421404887.hashdb",
        "goggame-1421404887.info"
    ];

    public override bool IsPlatformFoc(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.MinimumGameFilesExist(GameType.Foc, location))
            return false;

        if (!location.Name.Equals("EAWX", StringComparison.InvariantCultureIgnoreCase))
            return false;

        return ParentContainsGogFiles(location);
    }

    public override bool IsPlatformEaw(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.MinimumGameFilesExist(GameType.Eaw, location))
            return false;

        if (!location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase))
            return false;

        const string gogGameDll = "goggame-1421404887.dll";
        return DirectoryContainsFiles(location, [gogGameDll]) && ParentContainsGogFiles(location);
    }

    private static bool ParentContainsGogFiles(IDirectoryInfo gameLocation)
    {
        var parentDir = gameLocation.Parent;
        return parentDir is not null && DirectoryContainsFiles(parentDir, KnownGogFiles);
    }
}