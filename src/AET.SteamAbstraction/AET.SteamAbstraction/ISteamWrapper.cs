using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace AET.SteamAbstraction;

/// <summary>
/// .NET Wrapper to interact with the Steam Client
/// </summary>
public interface ISteamWrapper
{
    /// <summary>
    /// Returns <see langword="true"/> is Steam is installed on this machine; <see langword="false"/> otherwise.
    /// </summary>
    bool Installed { get; }

    /// <summary>
    /// Returns <see langword="true"/> is Steam is currently running; <see langword="false"/> otherwise.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Checks whether a given game is installed.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="manifest">The manifest of the installed game or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> is the game is installed; <see langword="false"/> otherwise.</returns>
    /// <exception cref="SteamException">if Steam is not installed.</exception>
    bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest? manifest);

    /// <summary>
    /// Starts the Steam client.
    /// </summary>
    /// <exception cref="SteamException">if Steam is not installed.</exception>
    void StartSteam();

    /// <summary>
    /// Returns a Task that completes when the Steam client is started and a user is logged in.
    /// <para>
    /// If the user requested Offline mode, the returned task completes when the Steam client is ready to use.
    /// </para>
    /// </summary>
    /// <param name="startIfNotRunning">When set to <see langword="true"/> Steam will be started if not already running.
    /// When set to <see langword="false"/> Steam must be started manually.</param>
    /// <param name="cancellation">A token that may be canceled to release the resources from waiting
    /// for Steam and complete the returned Task as canceled.</param>
    /// <returns>
    /// A task that completes when Steam is running and the user is logged in, or upon cancellation.
    /// </returns>
    Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning, CancellationToken cancellation = default);
}