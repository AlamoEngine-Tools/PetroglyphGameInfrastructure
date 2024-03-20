using System;

namespace PG.StarWarsGame.Infrastructure.Games;

/// <summary>
/// <see cref="PetroglyphException"/> for anything related to an <see cref="IGame"/>
/// </summary>
public class GameException : PetroglyphException
{
    /// <inheritdoc/>
    public GameException()
    {
    }

    /// <inheritdoc/>
    public GameException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public GameException(string message, Exception exception) : base(message, exception)
    {
    }
}