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

    protected virtual bool IsDataValid()
    {
        return true;
    }

    public bool IsValid(out ArgumentValidityStatus reason)
    {
        return IsValid(new ArgumentValidator(), out reason);
    }

    internal bool IsValid(IArgumentValidator validator, out ArgumentValidityStatus reason)
    {
        reason = validator.CheckArgument(this, out _, out _);
        if (reason != ArgumentValidityStatus.Valid)
            return false;
        if (IsDataValid())
            return true;
        reason = ArgumentValidityStatus.InvalidData;
        return false;
    }

    public abstract bool Equals(IGameArgument other);
}