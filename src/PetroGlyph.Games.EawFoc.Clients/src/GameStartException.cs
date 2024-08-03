using System;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// The exception that is thrown when a game was not started successfully.
/// </summary>
public sealed class GameStartException : ClientException
{
    /// <summary>
    /// The instance that was requested to start.
    /// </summary>
    public IPlayableObject Instance { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStartException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="instance">The instance that was requested to start.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public GameStartException(IPlayableObject instance, string message, Exception inner) : base(message, inner)
    {
        Instance = instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStartException"/> class with a specified error message.
    /// </summary>
    /// <param name="instance">The instance that was requested to start.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public GameStartException(IPlayableObject instance, string message) : base(message)
    {
        Instance = instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameStartException"/> class.
    /// </summary>
    /// <param name="instance">The instance that was requested to start.</param>
    public GameStartException(IPlayableObject instance)
    {
        Instance = instance;
    }
}