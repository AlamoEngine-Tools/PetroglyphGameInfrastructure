using System;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// A general exception for anything related to the Petroglyph game infrastructure.
/// </summary>
public class PetroglyphException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PetroglyphException"/> class. 
    /// </summary>
    public PetroglyphException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PetroglyphException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public PetroglyphException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PetroglyphException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public PetroglyphException(string message, Exception exception) : base(message, exception)
    {
    }
}