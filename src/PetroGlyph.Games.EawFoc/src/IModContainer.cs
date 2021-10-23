﻿using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc;

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
    /// Searches this instance for a <see cref="IMod"/> which matches a given <see cref="IModReference"/>.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/> to match.</param>
    /// <returns>The found mod.</returns>
    /// <exception cref="ModNotFoundException">Throws when unable to find a matching mod instance.</exception>
    IMod FindMod(IModReference modReference);

    /// <summary>
    /// Searches this instance for a <see cref="IMod"/> which matches a given <see cref="IModReference"/>.
    /// </summary>
    /// <param name="modReference">The <see cref="IModReference"/> to match.</param>
    /// <param name="mod">The mod instance which was found. <see langword="null"/>if no instance in this container matched.</param>
    /// <returns><see langword="true"/>if a matching mod could be found; <see langword="false"/> otherwise.</returns>
    /// <exception cref="ModNotFoundException">Throws when unable to find a matching mod instance.</exception>
    bool TryFindMod(IModReference modReference, out IMod? mod);
}