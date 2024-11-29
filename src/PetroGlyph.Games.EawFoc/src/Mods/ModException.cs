using System;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// The exception that is thrown for anything related to <see cref="IMod"/> or <see cref="IModReference"/>.
/// </summary>
public class ModException : PetroglyphException
{
    /// <summary>
    /// Gets the affected <see cref="IModReference"/>.
    /// </summary>
    public IModReference Mod { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModException"/> class of the specified mod reference.
    /// </summary>
    /// <param name="mod">The affected mod reference.</param>
    public ModException(IModReference mod)
    {
        Mod = mod;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModException"/> class of the specified mod reference with an error message.
    /// </summary>
    /// <param name="mod">The affected mod reference.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ModException(IModReference mod, string message) : base(message)
    {
        Mod = mod;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModException"/> class of the specified mod reference
    /// with an error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="mod">The affected mod reference.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="exception">The exception that is the cause of the current exception,
    /// or a <see langword="null"/> reference if no inner exception is specified.</param>
    public ModException(IModReference mod, string message, Exception exception) : base(message, exception)
    {
        Mod = mod;
    }
}