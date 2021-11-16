namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// The exception that is thrown when a <see cref="IGameArgument"/> or handling it caused an error.
/// </summary>
public class GameArgumentException : PetroglyphException
{
    /// <summary>
    /// The argument which caused the error.
    /// </summary>
    public IGameArgument Argument { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameArgumentException"/> class with the argument which caused this exception.
    /// </summary>
    /// <param name="argument">The argument which caused the error.</param>
    public GameArgumentException(IGameArgument argument)
    {
        Argument = argument;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameArgumentException"/> class with the argument which caused this exception
    /// and a specified error message.
    /// </summary>
    public GameArgumentException(IGameArgument argument, string message) : base(message)
    {
        Argument = argument;
    }
}