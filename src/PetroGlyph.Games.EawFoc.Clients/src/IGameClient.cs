using System;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// A game client is responsible for starting a Petroglyph Star Wars game,
/// as well as keeping track which game instances, started by this client, are currently running.
/// </summary>
public interface IGameClient : IDisposable
{
    /// <summary>
    /// Raised when a game process was started.
    /// </summary>
    event EventHandler<IGameProcess> GameStarted;

    /// <summary>
    /// Raised when a game was requested to start. The starting operation is cancellable.
    /// </summary>
    event EventHandler<GameStartingEventArgs> GameStarting;

    /// <summary>
    /// Gets the game instance that is associated to the client.
    /// </summary>
    public IGame Game { get; }

    /// <summary>
    /// Checks whether the debug executables of the game are present.
    /// </summary>
    /// <returns><see langword="true"/> if the debug executable are available; otherwise, <see langword="false"/>.</returns>
    bool IsDebugAvailable();

    /// <summary>
    /// Starts a new game process with no game arguments.
    /// </summary>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The game is not installed.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Play();

    /// <summary>
    /// Starts a new game process with the specified Mod and no other game arguments.
    /// </summary>
    /// <remarks>
    /// The method does use the mod's dependency information. Use <see cref="Play(IArgumentCollection)"/> for more complex cases.
    /// </remarks>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// <paramref name="mod"/> has a different game instance than the game instance associated to the client.
    /// OR
    /// <paramref name="mod"/> has an invalid path (such as containing space ' ' characters).
    /// OR
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The game is not installed.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Play(IMod mod);

    /// <summary>
    /// Starts a new game process with the specified game arguments.
    /// </summary>
    /// <param name="arguments">The arguments for starting the game.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// One or more argument of <paramref name="arguments"/> is invalid.
    /// OR
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Play(IArgumentCollection arguments);

    /// <summary>
    /// Starts a new game process using the debug executable with the specified game arguments.
    /// </summary>
    /// <param name="arguments">The arguments for starting the game.</param>
    /// <param name="fallbackToPlay">When set to <see langword="true"/> the game will start in release mode, if the debug build was not found.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// One or more argument of <paramref name="arguments"/> is invalid.
    /// OR
    /// Starting the game was cancelled by one <see cref="IGameClient.GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    /// <exception cref="NotSupportedException">The client does not support debugging the game.</exception>
    IGameProcess Debug(IArgumentCollection arguments, bool fallbackToPlay);
}