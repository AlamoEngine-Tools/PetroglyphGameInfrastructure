using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using AET.Modinfo.Spec.Equality;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// <see cref="IEqualityComparer{T}"/> for <see cref="IMod"/>, <see cref="IModIdentity"/> and <see cref="IModReference"/>.
/// </summary>
/// <remarks>For <see cref="IModIdentity"/> all operations are <see cref="IModIdentity.Version"/>-aware.
/// For <see cref="IModReference"/> all operations are not <see cref="IModReference.VersionRange"/>-aware.</remarks>
public sealed class ModEqualityComparer : IEqualityComparer<IMod>
{
    private readonly bool _includeDependencies;
    private readonly bool _includeGameReference;

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
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;

        if (!ModReferenceEqualityComparer.Default.Equals(x, y))
            return false;

        if (_includeGameReference)
        {
            if (!x.Game.Equals(y.Game))
                return false;
        }

        if (_includeDependencies)
        {
            if (!ModDependencyListEqualityComparer.Default.Equals(((IModIdentity)x).Dependencies,
                    ((IModIdentity)y).Dependencies))
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public int GetHashCode(IMod obj)
    {
        if (obj == null) 
            throw new ArgumentNullException(nameof(obj));
        var code = new HashCode();
        code.Add(ModReferenceEqualityComparer.Default.GetHashCode(obj));
        if (_includeGameReference)
            code.Add(obj.Game);
        if (_includeDependencies) 
            code.Add(ModDependencyListEqualityComparer.Default.GetHashCode(((IModIdentity)obj).Dependencies));
        return code.ToHashCode();
    }
}