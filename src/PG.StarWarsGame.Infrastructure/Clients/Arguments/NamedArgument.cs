namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Represents a game argument with the format Name=Value
/// </summary>
/// <remarks>
/// The value must not contain any space characters, as the game cannot handle them,
/// even when escaped or encapsulated with single or double quotes ('', "")
/// </remarks>
/// <typeparam name="T">Type of the Value.</typeparam>
public abstract class NamedArgument<T> : GameArgument<T> where T : notnull
{
    private protected NamedArgument(string name, T value, bool isDebug) : base(name, value, isDebug)
    {
    }
}