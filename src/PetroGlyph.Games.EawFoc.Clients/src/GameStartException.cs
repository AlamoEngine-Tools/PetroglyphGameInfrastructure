using System;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// The exception that is thrown when a game was not started successfully.
/// </summary>
public sealed class GameStartException : ClientException
{
    /// <summary>
    /// Gets the game which could not be started.
    /// </summary>
    public IGame Game { get; }

    /// <summary>
    /// Initializes a new game of the <see cref="GameStartException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="game">The game which could not be started.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public GameStartException(IGame game, string message, Exception inner) : base(message, inner)
    {
        Game = game;
    }

    /// <summary>
    /// Initializes a new game of the <see cref="GameStartException"/> class with a specified error message.
    /// </summary>
    /// <param name="game">The game which could not be started.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public GameStartException(IGame game, string message) : base(message)
    {
        Game = game;
    }
}