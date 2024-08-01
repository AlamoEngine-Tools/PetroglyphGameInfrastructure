using System;
using System.Collections.Generic;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Events arguments used for <see cref="IMod.DependenciesChanged"/>.
/// </summary>
public sealed class ModDependenciesChangedEventArgs : EventArgs
{
    /// <summary>
    /// The mods which raised the event.
    /// </summary>
    public IMod Mod { get; }

    /// <summary>
    /// A list of the old dependencies.
    /// </summary>
    public IList<ModDependencyEntry> OldDependencies { get; }

    /// <summary>
    /// A list of the new dependencies.
    /// </summary>
    public IList<ModDependencyEntry> NewDependencies { get; }

    /// <summary>
    /// The new layout.
    /// </summary>
    public DependencyResolveLayout Layout { get; }

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="mod">The mods which raised the event.</param>
    /// <param name="oldDependencies">A list of the old dependencies.</param>
    /// <param name="newDependencies">A list of the new dependencies.</param>
    /// <param name="layout">The new layout.</param>
    public ModDependenciesChangedEventArgs(
        IMod mod,
        IList<ModDependencyEntry> oldDependencies,
        IList<ModDependencyEntry> newDependencies,
        DependencyResolveLayout layout)
    {
        Mod = mod;
        OldDependencies = oldDependencies;
        NewDependencies = newDependencies;
        Layout = layout;
    }
}