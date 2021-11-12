using System;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public interface IGameArgument<out T> : IGameArgument where T : notnull
{
    new T Value { get; }
}

public interface IGameArgument : IEquatable<IGameArgument>
{
    ArgumentKind Kind { get; }

    bool DebugArgument { get; }

    string Name { get; }

    object Value { get; }

    string ValueToCommandLine();

    bool IsValid(out ArgumentValidityStatus reason);
}