using System.Collections.Generic;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

/// <summary>
/// Service that identifies which <see cref="GamePlatform"/> is present at a given installation location.
/// </summary>
internal interface IGamePlatformIdentifier
{
    /// <summary>
    /// Identifies the <see cref="GamePlatform"/> using a custom lookup strategy taken from <paramref name="lookupPlatforms"/>.
    /// </summary>
    /// <param name="type">The <see cref="GameType"/> of the installation.</param>
    /// <param name="location">The installation location. Note that during this method the reference might get changed!</param>
    /// <param name="lookupPlatforms">Ordered list of platforms to query. If the list contains <see cref="GamePlatform.Undefined"/>, all possible platforms will be queried in their default order.</param>
    /// <returns></returns>
    GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location, IList<GamePlatform> lookupPlatforms);
}