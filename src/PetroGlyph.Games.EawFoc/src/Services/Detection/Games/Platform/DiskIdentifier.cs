using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal class DiskIdentifier(IServiceProvider serviceProvider) : SpecificPlatformIdentifier(serviceProvider)
{
    public override bool IsPlatformFoc(ref IDirectoryInfo location)
    {
        // I don't know if there is a more precise way.
        // Thus, DiskVersion should always be the lowest priority searching the platform.
        return GameDetectorBase.GameExeExists(location, GameType.Foc) && GameDetectorBase.DataAndMegaFilesXmlExists(location);
    }

    public override bool IsPlatformEaw(ref IDirectoryInfo location)
    {
        if (!GameDetectorBase.GameExeExists(location, GameType.Eaw) || !GameDetectorBase.DataAndMegaFilesXmlExists(location))
            return false;

        return location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase);
    }
}