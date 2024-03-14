using System;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

/// <summary>
/// Result object used by <see cref="IGameDetector"/> containing all necessary information to create a game instance.
/// </summary>
public class GameDetectionResult
{
    /// <summary>
    /// The actual identity of the found game. This might differ from the actual detection request.
    /// </summary>
    public IGameIdentity GameIdentity { get; }

    /// <summary>
    /// Directory information of the game; <see langword="null"/> if an <see cref="Error"/> is present or <see cref="InitializationRequired"/> is <see langword="true"/>
    /// </summary>
    public IDirectoryInfo? GameLocation { get; }

    /// <summary>
    /// Indicates whether the detection hit a state where a game might be installed but requires initialization (like running the game once).
    /// </summary>
    public bool InitializationRequired { get; }

    /// <summary>
    /// Contains an <see cref="Exception"/> if there was any during the detection process; <see langword="null"/> otherwise.
    /// </summary>
    public Exception? Error { get; }

    private GameDetectionResult(GameType type)
    {
        GameIdentity = new GameIdentity(type, GamePlatform.Undefined);
    }

    /// <summary>
    /// Creates a new instance with given <see cref="IGameIdentity"/> and a non-null location.
    /// </summary>
    /// <param name="gameIdentity">The identity of the game.</param>
    /// <param name="location">The actual location of the game. Must not be <see langword="null"/></param>
    public GameDetectionResult(IGameIdentity gameIdentity, IDirectoryInfo location)
    {
        GameIdentity = gameIdentity;
        GameLocation = location ?? throw new ArgumentNullException(nameof(location));
    }

    internal GameDetectionResult(GameType type, Exception error) : this(type)
    {
        Error = error;
    }

    private GameDetectionResult(GameType type, bool initializationRequired) : this(type)
    {
        InitializationRequired = initializationRequired;
    }

    /// <summary>
    /// Creates a new instance where <see cref="GameLocation"/> is <see langword="null"/>
    /// </summary>
    /// <param name="type">The requested <see cref="GameType"/></param>
    public static GameDetectionResult NotInstalled(GameType type)
    {
        return new(type);
    }

    /// <summary>
    /// Creates a new instance where <see cref="InitializationRequired"/> is <see langword="true"/>
    /// </summary>
    /// <param name="type">The requested <see cref="GameType"/></param>
    public static GameDetectionResult RequiresInitialization(GameType type)
    {
        return new(type, true);
    }
}