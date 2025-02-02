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
        return GameDetectorBase.MinimumGameFilesExist(GameType.Foc, location);
    }

    public override bool IsPlatformEaw(ref IDirectoryInfo location)
    {
        return GameDetectorBase.MinimumGameFilesExist(GameType.Eaw, location);
    }
}