using System;
using System.Collections.Generic;
using System.Linq;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;

/// <summary>
/// Special argument which holds an ordered list of arguments to represent a mod chain.
/// </summary>
public sealed class ModArgumentList : GameArgument<IReadOnlyList<ModArgument>>
{
    /// <summary>
    /// Empty <see cref="ModArgumentList"/>.
    /// </summary>
    public static ModArgumentList Empty = new(new List<ModArgument>(0));

    /// <summary>
    /// This is always <see cref="ArgumentKind.ModList"/>.
    /// </summary>
    public override ArgumentKind Kind => ArgumentKind.ModList;

    /// <inheritdoc/>
    public override string Name => ArgumentNameCatalog.ModListArg;

    /// <summary>
    /// Creates a new argument from a given list of 
    /// </summary>
    /// <param name="mods">The mod arguments of this list.</param>
    public ModArgumentList(IReadOnlyList<ModArgument> mods) : base(mods)
    {
    }

    /// <inheritdoc/>
    public override string ValueToCommandLine()
    {
        // A ModList argument has no "value" which can be placed to the command line.
        // Its items need to be handled individually.
        return string.Empty;
    }

    /// <inheritdoc/>
    protected override bool IsDataValid()
    {
        // No other checks are done here.
        return Value.All(m => m.Kind == ArgumentKind.KeyValue);
    }

    /// <inheritdoc/>
    public override bool Equals(IGameArgument? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (other is not IGameArgument<IReadOnlyList<ModArgument>> otherModList)
            return false;
        return Kind == other.Kind && Value.SequenceEqual(otherModList.Value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, Name, Value);
    }
}