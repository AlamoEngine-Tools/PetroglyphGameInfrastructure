using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// This instance can be parent for one or many mods. 
/// </summary>
public interface IModContainer : IEnumerable<IMod>
{
    /// <summary>
    /// Notifies any handler when <see cref="Mods"/> was altered.
    /// </summary>
    event EventHandler<ModCollectionChangedEventArgs> ModsCollectionModified;

    /// <summary>
    /// Set of all mods this instance is associated with.
    /// For an <see cref="IGame"/> this is a flattened set of all Mods and their respective submods.
    /// </summary>
    IReadOnlyCollection<IMod> Mods { get; }

    /// <summary>
    /// Associates an <see cref="IMod"/> to the this <see cref="IModContainer"/>
    /// <remarks>
    /// This operation is "in-memory" only, meaning the <paramref name="mod"/>'s <see cref="IModIdentity.Dependencies"/>
    /// will NOT be updated.
    /// </remarks>
    /// </summary>
    /// <param name="mod">The mod instance</param>
    /// <returns><see langword="true"/> if the mod was added; otherwise <see langword="false"/> if the mod already existed.</returns>
    /// <exception cref="ModException"> if mods have different base games.</exception>
    bool AddMod(IMod mod);

    /// <summary>
    /// Removed an <see cref="IMod"/> from the this <see cref="IModContainer"/> 
    /// </summary>
    /// <param name="mod">The mod instance</param>
    /// <returns><see langword="true"/> if the mod was removed; otherwise <see langword="false"/> if the mod did not exists.</returns>
    /// <exception cref="ArgumentNullException"> when <paramref name="mod"/> is null</exception>
    bool RemoveMod(IMod mod);

    /// <summary>
    /// Searches <see cref="Mods"/> whether it contains the specified mod reference.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/> to find.</param>
    /// <returns>The found mod.</returns>
    /// <exception cref="ModNotFoundException"><paramref name="modReference"/> was not found in <see cref="Mods"/>.</exception>
    IMod FindMod(IModReference modReference);

    /// <summary>
    /// Searches <see cref="Mods"/> whether it contains the specified mod reference.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/> to find.</param>
    /// <param name="mod">When this method returns the variable contains the mod found or <see langword="null"/> if no <paramref name="modReference"/> was not found.</param>
    /// <returns><see langword="true"/>if a matching mod could be found; otherwise, <see langword="false"/>.</returns>
    bool TryFindMod(IModReference modReference, out IMod? mod);
}