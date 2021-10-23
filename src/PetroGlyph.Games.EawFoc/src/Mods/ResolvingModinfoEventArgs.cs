using System.ComponentModel;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// Cancellable event args when an <see cref="IMod.ResolvingModinfo"/> was raised.
/// </summary>
public class ResolvingModinfoEventArgs : CancelEventArgs
{
    /// <summary>
    /// The <see cref="IMod"/> which raised the event.
    /// </summary>
    public IMod Mod { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="mod">The callee mod.</param>
    public ResolvingModinfoEventArgs(IMod mod)
    {
        Mod = mod;
    }
}