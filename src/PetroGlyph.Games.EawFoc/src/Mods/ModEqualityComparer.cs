using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Spec.Equality;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// <see cref="IEqualityComparer{T}"/> for <see cref="IMod"/>, <see cref="IModIdentity"/> and <see cref="IModReference"/>.
/// </summary>
/// <remarks>For <see cref="IModIdentity"/> all operations are <see cref="IModIdentity.Version"/>-aware.
/// For <see cref="IModReference"/> all operations are not <see cref="IModReference.VersionRange"/>-aware.</remarks>
public sealed class ModEqualityComparer : IEqualityComparer<IMod>, IEqualityComparer<IModIdentity>, IEqualityComparer<IModReference>
{
    /// <summary>
    /// Default instance which checks for dependency and game equality.
    /// </summary>
    public static readonly ModEqualityComparer Default = new(true, true);
    /// <summary>
    /// Instance which includes the game reference but not dependencies.
    /// </summary>
    public static readonly ModEqualityComparer ExcludeDependencies = new(false, true);
    /// <summary>
    /// Instance which includes the dependencies but not the game reference.
    /// </summary>
    public static readonly ModEqualityComparer ExcludeGame = new(true, false);

    private readonly bool _includeDependencies;
    private readonly bool _includeGameReference;

    private readonly StringComparer _ignoreCaseComparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="includeDependencies">Specifies whether this instance shall include dependencies.</param>
    /// <param name="includeGameReference">Specifies whether this instance shall include the game reference.</param>
    public ModEqualityComparer(bool includeDependencies, bool includeGameReference)
    {
        _includeDependencies = includeDependencies;
        _includeGameReference = includeGameReference;
    }

    /// <summary>
    /// Checks two mods for equality.
    /// </summary>
    /// <param name="x">The first mod.</param>
    /// <param name="y">The second mod.</param>
    /// <returns><see langword="true"/>if mods are equal; <see langword="false"/> otherwise.</returns>
    public bool Equals(IMod? x, IMod? y)
    {
        if (x is null || y is null)
            return false;
        if (x == y)
            return true;

        if (!Equals((IModReference)x, y))
            return false;

        if (_includeGameReference)
        {
            if (!x.Game.Equals(y.Game))
                return false;
        }

        if (_includeDependencies)
        {
            if (!x.Dependencies.Count.Equals(y.Dependencies.Count))
                return false;

            if (!x.Dependencies.SequenceEqual(y.Dependencies))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public int GetHashCode(IMod obj)
    {
        var code = new HashCode();
        code.Add(obj.Identifier);
        if (_includeGameReference)
            code.Add(obj.Game);
        if (_includeDependencies)
        {
            var depHash = new HashCode();
            foreach (var dep in obj.Dependencies) 
                depHash.Add(dep);
            code.Add(depHash.ToHashCode());
        }
        return code.ToHashCode();
    }

    /// <summary>
    /// Checks two mod identities for equality.
    /// </summary>
    /// <remarks>Always checks for version equality.</remarks>
    /// <param name="x">The first mod.</param>
    /// <param name="y">The second mod.</param>
    /// <returns><see langword="true"/>if mods are equal; <see langword="false"/> otherwise.</returns>
    public bool Equals(IModIdentity? x, IModIdentity? y)
    {
        return !_includeDependencies
            ? new ModIdentityEqualityComparer(true, false, StringComparer.Ordinal).Equals(x, y)
            : ModIdentityEqualityComparer.Default.Equals(x, y);
    }

    /// <inheritdoc/>
    public int GetHashCode(IModIdentity obj)
    {
        return ModIdentityEqualityComparer.Default.GetHashCode(obj);
    }

    /// <summary>
    /// Default equality matching as specified in <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#iii22-modreference-equality"/>
    /// </summary>
    /// <param name="x">The first <see cref="IModReference"/>.</param>
    /// <param name="y">The second <see cref="IModReference"/>.</param>
    /// <returns><see langword="true"/>if both references are equal; <see langword="false"/> otherwise.</returns>
    public bool Equals(IModReference? x, IModReference? y)
    {
        if (x is null || y is null)
            return false;
        return ReferenceEquals(x, y) || new ModReference(x).Equals(new ModReference(y));
    }

    /// <inheritdoc/>
    public int GetHashCode(IModReference obj)
    {
        return new ModReference(obj).GetHashCode();
    }
}