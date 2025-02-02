using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
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
    /// Gets a set of all mods this mod container are associated to.
    /// </summary>
    IReadOnlyCollection<IMod> Mods { get; }

    /// <summary>
    /// Associates an <see cref="IMod"/> to the this <see cref="IModContainer"/>.
    /// </summary>
    /// <remarks>
    /// When this <see cref="IModContainer"/> is a <see cref="IMod"/>, <paramref name="mod"/> also gets added to <see cref="IPlayableObject.Game"/>.
    /// </remarks>
    /// <param name="mod">The mod instance</param>
    /// <returns><see langword="true"/> if the mod was added; otherwise <see langword="false"/> if the mod already existed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mod"/> is <see langword="null"/>.</exception>
    /// <exception cref="ModException"> <paramref name="mod"/> does not point to the same game reference as the container.</exception>
    bool AddMod(IMod mod);

    /// <summary>
    /// Removes a mod from the mod container.
    /// </summary>
    /// <param name="mod">The mod to remove</param>
    /// <returns><see langword="true"/> if <paramref name="mod"/> is successfully removed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mod"/> is <see langword="null"/>.</exception>
    bool RemoveMod(IMod mod);

    /// <summary>
    /// Searches <see cref="Mods"/> for a mod that matches the specified mod reference.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/> to search for.</param>
    /// <returns>The first mod that matched the mod reference, if found; otherwise, <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modReference"/> is <see langword="null"/>.</exception>
    IMod? FindMod(IModReference modReference);
}