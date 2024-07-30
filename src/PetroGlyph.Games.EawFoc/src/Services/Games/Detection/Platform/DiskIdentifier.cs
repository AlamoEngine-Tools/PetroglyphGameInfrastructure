using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal class DiskIdentifier : SpecificPlatformIdentifier
{
    public DiskIdentifier(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override bool IsPlatformFoc(ref IDirectoryInfo location)
    {
        // I don't know if there is a more precise way.
        // Thus DiskVersion should always be the lowest priority searching the platform.
        return GameDetector.GameExeExists(location, GameType.Foc);
    }

    public override bool IsPlatformEaw(ref IDirectoryInfo location)
    {
        if (!GameDetector.GameExeExists(location, GameType.Eaw))
            return false;

        return location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase);
    }
}