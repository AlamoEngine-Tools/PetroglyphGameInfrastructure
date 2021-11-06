using System;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public sealed class FlagArgument : GameArgument<bool>
{
    public override ArgumentKind Kind { get; }

    public override string Name { get; }

    public FlagArgument(string name, bool value, bool dashed = false, bool debug = false) : base(value, debug)
    {
        Name = name;
        Kind = dashed ? ArgumentKind.DashedFlag : ArgumentKind.Flag;
    }

    public override string ValueToCommandLine()
    {
        return Value ? Name : string.Empty;
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