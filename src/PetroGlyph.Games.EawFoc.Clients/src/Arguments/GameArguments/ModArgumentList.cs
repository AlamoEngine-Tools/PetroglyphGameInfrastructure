using System;
using System.Collections.Generic;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;

public sealed class ModArgumentList : GameArgument<IReadOnlyList<IGameArgument<string>>>
{
    public static ModArgumentList Empty = new(new List<NamedArgument<string>>(0));

    public override ArgumentKind Kind => ArgumentKind.ModList;

    public override string Name => ArgumentNameCatalog.ModListArg;

    public ModArgumentList(IReadOnlyList<NamedArgument<string>> mods) : base(mods)
    {
    }
    
    public override string ValueToCommandLine()
    {
        // A ModList argument has no "value" which can be placed to the command line.
        // Its items need to be handled individually.
        return string.Empty;
    }

    public override bool Equals(IGameArgument? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (other is not IGameArgument<IReadOnlyList<IGameArgument<string>>> otherModList)
            return false;
        return Kind == other.Kind && Value.SequenceEqual(otherModList.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, Name, Value);
    }
}