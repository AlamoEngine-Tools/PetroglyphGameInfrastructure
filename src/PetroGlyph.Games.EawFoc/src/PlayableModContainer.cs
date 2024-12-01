using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// Represent a <see cref="PlayableObject"/> which contains mods. This is an abstract class.
/// </summary>
/// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
public abstract class PlayableModContainer(IServiceProvider serviceProvider) : PlayableObject(serviceProvider), IModContainer
{
    /// <inheritdoc />
    public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;

    /// <summary>
    /// Shared internal (modifiable) set of mods.
    /// </summary>
    protected internal readonly HashSet<IMod> ModsInternal = new(new ModEqualityComparer(false, false));

    /// <inheritdoc />
    public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

    /// <inheritdoc />
    public IEnumerator<IMod> GetEnumerator()
    {
        return Mods.GetEnumerator();
    }

    /// <inheritdoc />
    public bool AddMod(IMod mod)
    {
        if (mod == null) 
            throw new ArgumentNullException(nameof(mod));
        if (!ReferenceEquals(Game, mod.Game))
            throw new ModException(mod, "Game instances of the two mods must be the same.");
        if (ReferenceEquals(this, mod) || (this is IModReference thisModRef && thisModRef.Equals(mod)))
            return false;
        var result = ModsInternal.Add(mod);
        if (result)
        {
            OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
            if (this is IMod containerIsMod)
                containerIsMod.Game.AddMod(mod);

        }
        return result;
    }

    /// <inheritdoc />
    public bool RemoveMod(IMod mod)
    {
        if (mod == null) 
            throw new ArgumentNullException(nameof(mod));
        var result = ModsInternal.Remove(mod);
        if (result)
            OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
        return result;
    }

    /// <inheritdoc />
    public IMod? FindMod(IModReference modReference)
    {
        if (modReference is null) 
            throw new ArgumentNullException(nameof(modReference));
        return Mods.FirstOrDefault(modReference.Equals);
    }

    /// <summary>
    /// Raises the <see cref="ModsCollectionModified"/> event for this instance.
    /// </summary>
    /// <param name="e">The event args to pass.</param>
    protected virtual void OnModsCollectionModified(ModCollectionChangedEventArgs e)
    {
        ModsCollectionModified?.Invoke(this, e);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}