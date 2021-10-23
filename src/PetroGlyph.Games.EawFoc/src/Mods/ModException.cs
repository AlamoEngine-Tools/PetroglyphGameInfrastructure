using System;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// <see cref="PetroglyphException"/> for anything related to <see cref="IMod"/> or <see cref="IModReference"/>.
/// </summary>
public class ModException : PetroglyphException
{
    /// <inheritdoc/>
    public ModException()
    {
    }

    /// <inheritdoc/>
    public ModException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public ModException(string message, Exception exception) : base(message, exception)
    {
    }
}