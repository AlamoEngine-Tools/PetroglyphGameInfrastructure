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
    /// Identifies the <see cref="GamePlatform"/> using a default lookup strategy
    /// </summary>
    /// <param name="type">The <see cref="GameType"/> of the installation.</param>
    /// <param name="location">The installation location. Note that during this method the reference might get changed!</param>
    /// <returns>The identified <see cref="GamePlatform"/>. Returns <see cref="GamePlatform.Undefined"/> is not platform could be identified.</returns>
    GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location);

    /// <summary>
    /// Identifies the <see cref="GamePlatform"/> using a custom lookup strategy taken from <paramref name="lookupPlatforms"/>.
    /// </summary>
    /// <param name="type">The <see cref="GameType"/> of the installation.</param>
    /// <param name="location">The installation location. Note that during this method the reference might get changed!</param>
    /// <param name="lookupPlatforms">The lookup strategy.
    /// If the list contains <see cref="GamePlatform.Undefined"/>, all possible platforms will be queried, by still remaining the order of the given list.
    /// If <see cref="GamePlatform.Undefined"/> is the only item, this method call equal to <see cref="GetGamePlatform(GameType,ref IDirectoryInfo)"/></param>
    /// <returns></returns>
    GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location, IList<GamePlatform> lookupPlatforms);
}