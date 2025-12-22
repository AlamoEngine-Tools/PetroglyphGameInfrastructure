#if !NET5_0_OR_GREATER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AET.Testing;

/// <summary>
/// An <see cref="IEqualityComparer{T}"/> that uses reference equality (<see cref="object.ReferenceEquals"/>)
/// instead of value equality(<see cref="Equals"/>) when comparing two object instances.
/// </summary>
/// <remarks>
/// The <see cref="ReferenceEqualityComparer"/> type cannot be instantiated.
/// Instead, use the <see cref="Instance"/> property to access the singleton instance of this type.
/// </remarks>
public sealed class ReferenceEqualityComparer : IEqualityComparer<object?>, IEqualityComparer
{
    /// <summary>
    /// Gets the singleton <see cref="ReferenceEqualityComparer"/> instance. 
    /// </summary>
    public static ReferenceEqualityComparer Instance { get; } = new();

    private ReferenceEqualityComparer()
    {
    }

    /// <summary>
    /// Determines whether two object references refer to the same object instance.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both <paramref name="x"/> and <paramref name="y"/>
    /// refer to the same object instance or if both are <see langword="null"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

    /// <summary>
    /// Returns a hash code for the specified object. The returned hash code is based on the object identity, not on the contents of the object.
    /// </summary>
    /// <param name="obj">The object for which to retrieve the hash code.</param>
    /// <returns>A hash code for the identity of <paramref name="obj"/>.</returns>
    public int GetHashCode(object? obj)
    {
        return RuntimeHelpers.GetHashCode(obj!);
    }
}
#endif