using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public abstract class GameArgument<T> : IGameArgument<T> where T : notnull
{
    public bool IsFlag => false;
    public abstract ArgumentKind Kind { get; }
    public bool DebugArgument { get; }
    public abstract string Name { get; }
    public T Value { get; }

    object IGameArgument.Value => Value;

    protected GameArgument(T value, bool isDebug = false)
    {
        Requires.NotNullAllowStructs(value, nameof(value));
        Value = value;
        DebugArgument = isDebug;
    }

    public abstract string ValueToCommandLine();

    public abstract bool Equals(IGameArgument other);
}