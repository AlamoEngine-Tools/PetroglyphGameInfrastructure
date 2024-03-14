using System;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure;

/// <summary>
/// <see cref="EventArgs"/> for a <see cref="IModContainer.ModsCollectionModified"/> event.
/// </summary>
public sealed class ModCollectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// The performed action which raised the event.
    /// </summary>
    public ModCollectionChangedAction Action { get; }

    /// <summary>
    /// The removed/added mod.
    /// </summary>
    public IMod Mod { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="mod">The removed/added mod.</param>
    /// <param name="action">The performed action which raised the event.</param>
    public ModCollectionChangedEventArgs(IMod mod, ModCollectionChangedAction action)
    {
        Mod = mod;
        Action = action;
    }
}