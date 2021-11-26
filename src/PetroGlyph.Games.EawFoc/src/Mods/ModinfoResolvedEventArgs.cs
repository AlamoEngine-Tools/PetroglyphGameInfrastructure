using System;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// Event arguments used for <see cref="IMod.ModinfoResolved"/>.
/// </summary>
public class ModinfoResolvedEventArgs : EventArgs
{
    /// <summary>
    /// The <see cref="IMod"/> which raised the event.
    /// </summary>
    public IMod Mod { get; }

    /// <summary>
    /// The modinfo which got resolved.
    /// </summary>
    public IModinfo Modinfo { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="mod">The <see cref="IMod"/> which raised the event.</param>
    /// <param name="modinfo">The modinfo which got resolved.</param>
    public ModinfoResolvedEventArgs(IMod mod, IModinfo modinfo)
    {
        Mod = mod;
        Modinfo = modinfo;
    }
}