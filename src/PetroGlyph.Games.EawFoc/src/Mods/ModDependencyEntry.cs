using System;
using System.Diagnostics.CodeAnalysis;
using EawModinfo.Spec;
using Semver;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Object which represents an <see cref="IMod"/> in a <see cref="IMod.Dependencies"/> list,
/// by keeping the original version range property from <see cref="IModReference.VersionRange"/>
/// </summary>
/// <remarks>Equality is implemented as specified in
/// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#the-version-range-property"/>.
/// Meaning the <see cref="VersionRange"/> gets totally ignored.</remarks>
public sealed class ModDependencyEntry : IEquatable<ModDependencyEntry>
{
    /// <summary>
    /// The dependency instance.
    /// </summary>
    public IMod Mod { get; }

    /// <summary>
    /// The original version range instance.
    /// </summary>
    public SemVersionRange? VersionRange { get; }

    /// <summary>
    /// Creates a new instance with <see langword="null"/> for <see cref="VersionRange"/>.
    /// </summary>
    /// <param name="mod">The dependency instance.</param>
    public ModDependencyEntry(IMod mod) : this(mod, null)
    {
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="mod">The dependency instance.</param>
    /// <param name="range">The original version range instance.</param>
    public ModDependencyEntry(IMod mod, SemVersionRange? range)
    {
        Mod = mod ?? throw new ArgumentNullException(nameof(mod));
        VersionRange = range;
    }

    /// <inheritdoc/>
    public bool Equals(ModDependencyEntry? other)
    {
        if (other is null)
            return false;
        return ReferenceEquals(this, other) || Mod.Equals(other.Mod);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (this == obj)
            return true;
        return obj is ModDependencyEntry other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Mod.GetHashCode();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{Mod.Identifier}:{VersionRange}";
    }
}