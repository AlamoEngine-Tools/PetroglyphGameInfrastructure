using System.ComponentModel;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Provides data for the <see cref="IGameClient.GameStarting"/> event. 
/// </summary>
public sealed class GameStartingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets the game instance requested to start.
    /// </summary>
    public IGame Game { get; }

    /// <summary>
    /// Gets the requested game arguments 
    /// </summary>
    public ArgumentCollection GameArguments { get; }

    /// <summary>
    /// Gets the requested build type of the game.
    /// </summary>
    public GameBuildType BuildType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStartingEventArgs"/> with the specified game launch information.
    /// </summary>
    /// <param name="game">The game instance requested to start.</param>
    /// <param name="arguments">The requested game arguments.</param>
    /// <param name="buildType">The requested build type of the game.</param>
    public GameStartingEventArgs(IGame game, ArgumentCollection arguments, GameBuildType buildType)
    {
        Game = game;
        GameArguments = arguments;
        BuildType = buildType;
    }
}