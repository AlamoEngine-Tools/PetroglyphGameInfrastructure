using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

internal interface ISpecificPlatformIdentifier
{
    bool IsPlatform(GameType type, ref IDirectoryInfo location);
}