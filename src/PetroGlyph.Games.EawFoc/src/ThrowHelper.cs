using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Contains helpers for throwing common exceptions.
/// </summary>
internal static class ThrowHelper
{
    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.
    public static void ThrowIfCollectionNullOrEmpty([NotNull] ICollection? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
        if (argument.Count == 0)
            throw new ArgumentException("The value cannot be an empty collection.", nameof(paramName));
    }

    public static void ThrowIfCollectionNullOrEmpty<T>([NotNull] ICollection<T>? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
        if (argument.Count == 0)
            throw new ArgumentException("The value cannot be an empty collection.", nameof(paramName));
    }
#pragma warning restore CS8777 // Parameter must have a non-null value when exiting.
}