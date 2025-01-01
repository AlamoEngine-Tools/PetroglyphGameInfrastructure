namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// The exception that is thrown when a <see cref="GameArgument"/> or handling it caused an error.
/// </summary>
public sealed class GameArgumentException : PetroglyphException
{
    /// <summary>
    /// The argument which caused the error.
    /// </summary>
    public GameArgument Argument { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameArgumentException"/> class with the argument which caused this exception
    /// and a specified error message.
    /// </summary>
    public GameArgumentException(GameArgument argument, string message) : base(message)
    {
        Argument = argument;
    }
}