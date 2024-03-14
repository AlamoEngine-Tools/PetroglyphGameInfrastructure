using System;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// The exception that is thrown when a Client operation caused an error.
/// </summary>
public class ClientException : Exception
{
    /// <summary>
    /// The instance that was requested to start.
    /// </summary>
    public ClientException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ClientException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public ClientException(string message, Exception inner) : base(message, inner)
    {
    }
}