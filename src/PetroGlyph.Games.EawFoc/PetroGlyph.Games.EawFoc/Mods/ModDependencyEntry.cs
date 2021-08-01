using System;
using EawModinfo.Spec;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    /// <summary>
    /// Object which represents an <see cref="IMod"/> in a <see cref="IMod.Dependencies"/> list,
    /// by keeping the original version range property from <see cref="IModReference.VersionRange"/>
    /// </summary>
    /// <remarks>Equality is implemented as specified in
    /// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#the-version-range-property"/>.
    /// Meaning the <see cref="VersionRange"/> get totally ignored.</remarks>
    public sealed class ModDependencyEntry : IEquatable<ModDependencyEntry>
    {
        /// <summary>
        /// The dependency instance.
        /// </summary>
        public IMod Mod { get; }

        /// <summary>
        /// The original version range instance.
        /// </summary>
        public SemanticVersioning.Range? VersionRange { get; }

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
        public ModDependencyEntry(IMod mod, SemanticVersioning.Range? range)
        {
            Requires.NotNull(mod, nameof(mod));
            Mod = mod;
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
    }
}