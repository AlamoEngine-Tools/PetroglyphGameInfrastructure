using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Clients;

/// <summary>
/// A <see cref="IGameClient"/> is responsible for starting a Petroglyph Star Wars game,
/// as well as keeping track which game instances, started by this client, are currently running.
/// </summary>
public interface IGameClient
{
    /// <summary>
    /// Event gets raised when a game process was started.
    /// </summary>
    event EventHandler<IGameProcess> GameStarted;

    /// <summary>
    /// Event gets raised when a game was requested to start. The starting operation is cancellable.
    /// </summary>
    event EventHandler<GameStartingEventArgs> GameStarting;

    /// <summary>
    /// Event gets raised when a game, started by this client instance, was terminated.
    /// </summary>
    event EventHandler<IGameProcess> GameClosed;

    /// <summary>
    /// Set of arguments which shall be passed when no custom arguments are specified.
    /// </summary>
    IArgumentCollection DefaultArguments { get; }

    /// <summary>
    /// Collection which holds the currently running game instances.
    /// </summary>
    IReadOnlyCollection<IGameProcess> RunningInstances { get; }

    /// <summary>
    /// Set of <see cref="GamePlatform"/> supported by this client.
    /// </summary>
    ISet<GamePlatform> SupportedPlatforms { get; }

    /// <summary>
    /// Plays the given <paramref name="instance"/> with <see cref="DefaultArguments"/>.
    /// <para>
    /// If <paramref name="instance"/> in an <see cref="IMod"/> the arguments passed
    /// will be <see cref="DefaultArguments"/> merged with the dependency chain of the given mod.
    /// </para>
    /// </summary>
    /// <param name="instance">The game or mod to start.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// The game's platform is not supported by this client.
    /// OR
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Play(IPlayableObject instance);

    /// <summary>
    /// Plays the given <paramref name="instance"/> with given <paramref name="arguments"/>.
    /// <para>
    /// If <paramref name="instance"/> in an <see cref="IMod"/> the arguments passed,
    /// are required to have all required mod related arguments set..
    /// </para>
    /// </summary>
    /// <param name="instance">The game or mod to start.</param>
    /// <param name="arguments">The arguments for starting the game.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// The game's platform is not supported by this client.
    /// OR
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Play(IPlayableObject instance, IArgumentCollection arguments);
}