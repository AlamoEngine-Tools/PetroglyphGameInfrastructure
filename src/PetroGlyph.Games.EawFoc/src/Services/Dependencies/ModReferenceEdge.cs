using System;
using QuikGraph;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal sealed class ModReferenceEdge(GraphModReference source, GraphModReference target) : Edge<GraphModReference>(source, target), IEquatable<ModReferenceEdge>
{
    public bool Equals(ModReferenceEdge? other)
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
        return obj is ModReferenceEdge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, Target);
    }
}