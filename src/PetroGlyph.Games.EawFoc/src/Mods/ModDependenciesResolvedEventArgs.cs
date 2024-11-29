using System;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Events arguments used when a mod has its dependencies resolved.
/// </summary>
public sealed class ModDependenciesResolvedEventArgs(IMod mod) : EventArgs
{
    /// <summary>
    /// Gets the mod which raised the event.
    /// </summary>
    public IMod Mod { get; } = mod ?? throw new ArgumentNullException(nameof(mod));
}