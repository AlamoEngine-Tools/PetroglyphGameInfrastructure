using System;
using QuikGraph;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal sealed class ModDependencyGraphEdge(ModDependencyGraphVertex source, ModDependencyGraphVertex target) : Edge<ModDependencyGraphVertex>(source, target), IEquatable<ModDependencyGraphEdge>
{
    public bool Equals(ModDependencyGraphEdge? other)
    {
        if (other is null)
            return false;
        return Source.Equals(other.Source) && Target.Equals(other.Target);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj is ModDependencyGraphEdge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, Target);
    }
}