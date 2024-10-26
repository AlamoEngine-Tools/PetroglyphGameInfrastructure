using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

/// <summary>
/// Service that identifies which <see cref="GamePlatform"/> is present at a given installation location.
/// </summary>
internal interface IGamePlatformIdentifier
{
    /// <summary>
    /// Identifies the <see cref="GamePlatform"/> from a specified game location.
    /// </summary>
    /// <param name="type">The <see cref="GameType"/> of the installation.</param>
    /// <param name="location">The variable containing a reference to the installation location. During this operation the reference might get changed.</param>
    /// <returns>The identified game platform. <see cref="GamePlatform.Undefined"/> if no platform could be identified.</returns>
    GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location);
}