using System;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public abstract class NamedArgument<T> : GameArgument<T> where T : notnull
{
    public override ArgumentKind Kind => ArgumentKind.KeyValue;

    public override string Name { get; }

    protected NamedArgument(string name, T value, bool isDebug) : base(value, isDebug)
    {
        Name = name;
    }

    public override bool Equals(IGameArgument? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Kind == other.Kind && Name == other.Name && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is IGameArgument other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, Name, Value);
    }
}