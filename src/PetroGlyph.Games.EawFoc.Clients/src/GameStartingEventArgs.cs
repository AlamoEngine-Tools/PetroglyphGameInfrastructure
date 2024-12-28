using System.ComponentModel;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Cancelable event handler when a game was requested to start.
/// </summary>
public sealed class GameStartingEventArgs : CancelEventArgs
{
    /// <summary>
    /// The instance requested to start.
    /// </summary>
    public IGame Game { get; }

    /// <summary>
    /// The requested game arguments 
    /// </summary>
    public IArgumentCollection GameArguments { get; }

    /// <summary>
    /// The requested <see cref="GameBuildType"/> of the game.
    /// </summary>
    public GameBuildType BuildType { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="game">The instance requested to start.</param>
    /// <param name="arguments">The requested game arguments </param>
    /// <param name="buildType">The requested <see cref="GameBuildType"/> of the game.</param>
    public GameStartingEventArgs(IGame game, IArgumentCollection arguments, GameBuildType buildType)
    {
        Game = game;
        GameArguments = arguments;
        BuildType = buildType;
    }
}