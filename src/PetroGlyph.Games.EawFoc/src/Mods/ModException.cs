using System;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// <see cref="PetroglyphException"/> for anything related to <see cref="IMod"/> or <see cref="IModReference"/>.
/// </summary>
public class ModException : PetroglyphException
{
    /// <summary>
    /// The <see cref="IModReference"/> which caused the exception.
    /// </summary>
    public IModReference Mod { get; }

    /// <summary>
    /// Creates a new exception caused by <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">The mod which caused the exception.</param>
    public ModException(IModReference mod)
    {
        Mod = mod;
    }

    /// <summary>
    /// Creates a new exception caused by <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">The mod which caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ModException(IModReference mod, string message) : base(message)
    {
        Mod = mod;
    }

    /// <summary>
    /// Creates a new exception caused by <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">The mod which caused the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception,
    /// or a <see langword="null"/> reference if no inner exception is specified.</param>
    public ModException(IModReference mod, string message, Exception exception) : base(message, exception)
    {
        Mod = mod;
    }
}