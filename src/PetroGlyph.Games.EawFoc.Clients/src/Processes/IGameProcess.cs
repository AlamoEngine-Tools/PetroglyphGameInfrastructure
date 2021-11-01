using System;
using System.Threading;
using System.Threading.Tasks;
#if NETSTANDARD2_0_OR_GREATER
#endif

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

/// <summary>
/// High-Level representation of a game process.
/// </summary>
public interface IGameProcess
{
    /// <summary>
    /// Events gets raised if the game was terminated.
    /// <para>
    /// If the process is already terminated, the handler will be run immediately.
    /// </para>
    /// </summary>
    event EventHandler Closed;
    
    /// <summary>
    /// Information about the running game instance.
    /// </summary>
    GameProcessInfo ProcessInfo { get; }

    /// <summary>
    /// Current state about the game process.
    /// </summary>
    GameProcessState State { get; }

    /// <summary>
    /// Terminates the game process.
    /// </summary>
    void Exit();

    /// <summary>
    /// Instructs the process component to wait for the associated process to exit,
    /// or for the <paramref name="cancellationToken"/> to be cancelled.
    /// </summary>
    /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
    /// <returns>A task that will complete when the process has exited,
    /// cancellation has been requested, or an error occurs.</returns>
    Task WaitForExitAsync(CancellationToken cancellationToken = default);
}