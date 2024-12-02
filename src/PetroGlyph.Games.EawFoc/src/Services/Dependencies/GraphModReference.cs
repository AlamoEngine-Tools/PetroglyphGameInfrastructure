using System;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal sealed class GraphModReference(IMod mod, DependencyKind kind) : IEquatable<GraphModReference>
{
    public IMod Mod { get; } = mod;

    public DependencyKind Kind { get; } = kind;

    public bool Equals(GraphModReference? other)
    {
        if (other is null) 
            return false;
        return ReferenceEquals(this, other) || Mod.Equals(other.Mod);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is GraphModReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Mod.GetHashCode();
    }
}