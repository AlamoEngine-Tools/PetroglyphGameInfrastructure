using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

/// <summary>
/// Low-Level service that actually starts the game process.
/// </summary>
public interface IGameProcessLauncher
{
    /// <summary>
    /// Starts the given <paramref name="executable"/> with information provided by <paramref name="processInfo"/>
    /// </summary>
    /// <param name="executable">The executable to start.</param>
    /// <param name="processInfo">Process information including launch arguments.</param>
    /// <returns>The created game process.</returns>
    /// <exception cref="GameStartException">The process could not be created.</exception>
    IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo);
}