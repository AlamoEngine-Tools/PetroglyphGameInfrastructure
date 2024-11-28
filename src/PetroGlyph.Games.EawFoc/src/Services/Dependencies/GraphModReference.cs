using System;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

internal sealed class GraphModReference(IModReference modReference, DependencyKind kind) : IEquatable<GraphModReference>
{
    public IModReference ModReference { get; } = modReference;

    public DependencyKind Kind { get; } = kind;

    public bool Equals(GraphModReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ModReference.Equals(other.ModReference);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is GraphModReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ModReference.GetHashCode();
    }
}