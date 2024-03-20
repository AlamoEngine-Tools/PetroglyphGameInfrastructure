namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

/// <summary>
/// The sate of a game process.
/// </summary>
public enum GameProcessState
{
    /// <summary>
    /// The process is currently running.
    /// </summary>
    Running,
    /// <summary>
    /// The game process was closed.
    /// </summary>
    Closed
}