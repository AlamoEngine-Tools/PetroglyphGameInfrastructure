using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal interface IGameProcessLauncher
{
    /// <summary>
    /// Starts a new process of the specified executable.
    /// </summary>
    /// <remarks>
    /// The working directory of the new process is set to the executable location.
    /// </remarks>
    /// <exception cref="GameStartException">The provided game arguments are invalid.</exception>
    /// <exception cref="GameStartException">The process could not be started.</exception>
    IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo);
}