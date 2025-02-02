using System;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// The exception that is thrown for anything related to Petroplyph Star Wars game.
/// </summary>
public class GameException : PetroglyphException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class.
    /// </summary>
    public GameException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public GameException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception.</param>
    public GameException(string message, Exception exception) : base(message, exception)
    {
    }
}