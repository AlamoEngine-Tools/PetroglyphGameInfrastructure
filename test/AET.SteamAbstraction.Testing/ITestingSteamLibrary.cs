using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using System.Collections.Generic;

namespace AET.SteamAbstraction.Testing;

/// <summary>
/// Represents a testing Steam game library that can be manipulated for testing purposes.
/// </summary>
public interface ITestingSteamLibrary : ISteamLibrary
{
    /// <summary>
    /// Installs a game with the specified game ID, name, and manifest file name, and returns the associated
    /// Steam application manifest.
    /// </summary>
    /// <param name="id">The ID of the game to install.</param>
    /// <param name="gameName">The display name of the game to be installed.</param>
    /// <param name="appManifestName">The name of the manifest file to use for installation.</param>
    /// <returns>A <see cref="SteamAppManifest"/> object representing the installed game's manifest.</returns>
    SteamAppManifest InstallGame(uint id, string gameName, string appManifestName);

    /// <summary>
    /// Installs a game with the specified game ID, name, number of installed depots and app state, and returns the associated
    /// Steam application manifest.
    /// </summary>
    /// <param name="id">The ID of the game to install.</param>
    /// <param name="gameName">The display name of the game to be installed.</param>
    /// <param name="numberDepots">The number of depots to be installed.</param>
    /// <param name="appState">The state of the installed game.</param>
    /// <returns>A <see cref="SteamAppManifest"/> object representing the installed game's manifest.</returns>
    SteamAppManifest InstallGame(
        uint id, 
        string gameName, 
        uint numberDepots = 1,
        SteamAppState appState = SteamAppState.StateFullyInstalled);

    /// <summary>
    /// Installs a game with the specified game ID, name, a list of installed depots and app state, and returns the associated
    /// Steam application manifest.
    /// </summary>
    /// <param name="id">The ID of the game to install.</param>
    /// <param name="gameName">The display name of the game to be installed.</param>
    /// <param name="depots">A list of depots to be installed.</param>
    /// <param name="appState">The state of the installed game.</param>
    /// <returns>A <see cref="SteamAppManifest"/> object representing the installed game's manifest.</returns>
    SteamAppManifest InstallGame(
        uint id,
        string gameName,
        IList<uint> depots,
        SteamAppState appState = SteamAppState.StateFullyInstalled);

    /// <summary>
    /// Creates an intentionally corrupted game manifest.
    /// </summary>
    void InstallCorruptApp();
}