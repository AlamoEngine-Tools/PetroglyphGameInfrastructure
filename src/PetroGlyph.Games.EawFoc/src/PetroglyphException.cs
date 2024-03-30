using System;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// General exception for anything Petroglyph related.
/// </summary>
public class PetroglyphException : Exception
{
    /// <inheritdoc/>
    public PetroglyphException()
    {
    }

    /// <inheritdoc/>
    public PetroglyphException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public PetroglyphException(string message, Exception exception) : base(message, exception)
    {
    }
}