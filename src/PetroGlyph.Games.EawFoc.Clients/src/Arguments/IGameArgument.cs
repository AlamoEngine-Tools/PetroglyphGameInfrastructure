using System;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Represents a typed argument for a Petroglyph Star Wars game.
/// </summary>
/// <typeparam name="T">The type of the argument's value.</typeparam>
public interface IGameArgument<out T> : IGameArgument where T : notnull
{
    /// <summary>
    /// The value of the argument.
    /// </summary>
    new T Value { get; }
}

/// <summary>
/// Represents a argument for a Petroglyph Star Wars game.
/// </summary>
public interface IGameArgument : IEquatable<IGameArgument>
{
    /// <summary>
    /// Flag indicating how this arguments shall be handled.
    /// </summary>
    ArgumentKind Kind { get; }

    /// <summary>
    /// Indicates this argument can only be used in debug mode if <see langword="true"/>.
    /// </summary>
    bool DebugArgument { get; }

    /// <summary>
    /// The name of the argument.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The value of the argument.
    /// </summary>
    object Value { get; }

    /// <summary>
    /// Converts the value, and only the value, to a string representation which can be used as a command line argument.
    /// </summary>
    /// <remarks>
    /// Arguments of kind <see cref="ArgumentKind.ModList"/> are expected to return an empty string.
    /// </remarks>
    /// <returns>The string representation of the value.</returns>
    string ValueToCommandLine();

    /// <summary>
    /// Checks whether this argument has correct data. This checks the <see cref="Name"/> as well as <see cref="Value"/>.
    /// </summary>
    /// <param name="reason">Detailed status of the validation.</param>
    /// <returns><see langword="true"/> when the argument is valid; <see langword="false"/> otherwise.</returns>
    bool IsValid(out ArgumentValidityStatus reason);
}