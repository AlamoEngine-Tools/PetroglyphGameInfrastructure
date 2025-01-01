using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

/// <summary>
/// Special argument which holds an ordered list of arguments to represent a mod chain.
/// </summary>
internal sealed class ModArgumentList : GameArgument<IReadOnlyList<ModArgument>>
{
    /// <summary>
    /// Empty <see cref="ModArgumentList"/>.
    /// </summary>
    public static ModArgumentList Empty = new(new List<ModArgument>(0));

    internal ModArgumentList(IReadOnlyList<ModArgument> mods) : base(GameArgumentNames.ModListArg, mods)
    {
    }

    /// <inheritdoc/>
    internal override string ValueToCommandLine()
    {
        // The value created by the ArgumentCommandLineBuilder,
        // thus we return just an empty string.
        return string.Empty;
    }

    private protected override bool EqualsValue(GameArgument other)
    {
        if (other is not ModArgumentList otherModList)
            return false;
        if (Value.Count != otherModList.Value.Count)
            return false;
        return Value.SequenceEqual(otherModList.Value);
    }

    private protected override int ValueHash()
    {
        var hashCode = new HashCode();
        foreach (var mod in Value) 
            hashCode.Add(mod);
        return hashCode.ToHashCode();
    }
}