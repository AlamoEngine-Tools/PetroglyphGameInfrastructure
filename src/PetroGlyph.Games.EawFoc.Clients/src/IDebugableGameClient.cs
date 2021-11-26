using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Clients;

/// <summary>
/// <see cref="IGameClient"/> with additional support to start the debug builds of the games.
/// </summary>
public interface IDebugableGameClient : IGameClient
{
    /// <summary>
    /// Checks whether the debug build is available.
    /// </summary>
    /// <param name="instance">The requested game instance.</param>
    /// <returns><see langowrd="true"/> if the debug build was found; <see langowrd="false"/> otherwise.</returns>
    bool IsDebugAvailable(IPlayableObject instance);

    /// <summary>
    /// Plays the given <paramref name="instance"/> with <see cref="IGameClient.DefaultArguments"/> in debug mode.
    /// <para>
    /// If <paramref name="instance"/> in an <see cref="IMod"/> the arguments passed
    /// will be <see cref="IGameClient.DefaultArguments"/> merged with the dependency chain of the given mod.
    /// </para>
    /// </summary>
    /// <param name="instance">The game or mod to start.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// The game's platform is not supported by this client.
    /// OR
    /// Starting the game was cancelled by one <see cref="IGameClient.GameStarting"/> handler.
    /// OR
    /// The debug executable was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Debug(IPlayableObject instance);

    /// <summary>
    /// Plays the given <paramref name="instance"/> with given <param name="arguments"></param>.
    /// <para>
    /// If <paramref name="instance"/> in an <see cref="IMod"/> the arguments passed,
    /// are required to have all required mod related arguments set..
    /// </para>
    /// </summary>
    /// <param name="instance">The game or mod to start.</param>
    /// <param name="arguments">The arguments for starting the game.</param>
    /// <param name="fallbackToPlay">When set to <see langword="true"/> the game will start in release mode,
    /// if the debug build was not found.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// The game's platform is not supported by this client.
    /// OR
    /// Starting the game was cancelled by one <see cref="IGameClient.GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    IGameProcess Debug(IPlayableObject instance, IArgumentCollection arguments, bool fallbackToPlay);
}