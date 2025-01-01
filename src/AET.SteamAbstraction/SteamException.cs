using System;

namespace AET.SteamAbstraction;

/// <summary>
/// The exception that is thrown if anything Steam related fails.
/// </summary>
public class SteamException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SteamException"/> class.
    /// </summary>
    public SteamException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SteamException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public SteamException(string? message, Exception? exception) : base(message, exception)
    {
    }
}