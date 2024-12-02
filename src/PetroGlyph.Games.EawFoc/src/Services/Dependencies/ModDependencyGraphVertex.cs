using System;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal sealed class ModDependencyGraphVertex(IMod mod, DependencyKind kind) : IEquatable<ModDependencyGraphVertex>
{
    public IMod Mod { get; } = mod;

    public DependencyKind Kind { get; } = kind;

    public bool Equals(ModDependencyGraphVertex? other)
    {
        if (other is null) 
            return false;
        return ReferenceEquals(this, other) || Mod.Equals(other.Mod);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ModDependencyGraphVertex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Mod.GetHashCode();
    }
}