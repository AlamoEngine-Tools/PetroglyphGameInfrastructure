using System;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Events arguments used for <see cref="IMod.DependenciesResolved"/>.
/// </summary>
public sealed class ModDependenciesResolvedEventArgs(IMod mod) : EventArgs
{
    /// <summary>
    /// Gets the mod which raised the event.
    /// </summary>
    public IMod Mod { get; } = mod ?? throw new ArgumentNullException(nameof(mod));
}