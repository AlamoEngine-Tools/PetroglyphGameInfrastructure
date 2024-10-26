using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// Result object used by <see cref="IGameDetector"/> containing all necessary information to create a game instance.
/// </summary>
public sealed class GameDetectionResult
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="GameDetectionResult"/> points to an installed game.
    /// </summary>
    [MemberNotNullWhen(true, nameof(GameLocation))]
    public bool Installed => GameLocation is not null;

    /// <summary>
    /// Gets the actual identity of the found game. This might differ from the actual detection request.
    /// </summary>
    public IGameIdentity GameIdentity { get; }

    /// <summary>
    /// Gets the directory information of the game.
    /// </summary>
    public IDirectoryInfo? GameLocation { get; }

    /// <summary>
    /// Gets a value indicating whether the detected game state requires an initialization (like running the game once).
    /// </summary>
    public bool InitializationRequired { get; }

    private GameDetectionResult(IGameIdentity gameIdentity, IDirectoryInfo? location, bool requiresInitialization)
    {
        GameIdentity = gameIdentity;
        GameLocation = location;
        InitializationRequired = requiresInitialization;
    }

    /// <summary>
    /// Creates a new instance of <see cref="GameDetectionResult"/> of a detected game.
    /// </summary>
    /// <param name="gameIdentity">The identity of the detected game.</param>
    /// <param name="location">The location of the detected game.</param>
    public static GameDetectionResult FromInstalled(GameIdentity gameIdentity, IDirectoryInfo location)
    {
        if (gameIdentity == null) 
            throw new ArgumentNullException(nameof(gameIdentity));
        if (location == null) 
            throw new ArgumentNullException(nameof(location));
        return new(gameIdentity, location, false);
    }

    /// <summary>
    /// Creates a new instance of <see cref="GameDetectionResult"/> of an undetected game.
    /// </summary>
    /// <param name="type">The requested <see cref="GameType"/></param>
    public static GameDetectionResult NotInstalled(GameType type)
    {
        return new(new GameIdentity(type, GamePlatform.Undefined), null, false);
    }

    /// <summary>
    /// Creates a new instance of <see cref="GameDetectionResult"/> of an uninitialized game.
    /// </summary>
    /// <param name="type">The requested <see cref="GameType"/>.</param>
    public static GameDetectionResult RequiresInitialization(GameType type)
    {
        return new(new GameIdentity(type, GamePlatform.Undefined), null, true);
    }
}