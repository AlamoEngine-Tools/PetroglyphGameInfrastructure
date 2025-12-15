using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using System.Collections.Generic;

namespace AET.SteamAbstraction.Testing;

public interface ITestingSteamLibrary : ISteamLibrary
{
    SteamAppManifest InstallGame(uint id, string gameName, string appManifestName);

    SteamAppManifest InstallGame(
        uint id, 
        string gameName, 
        uint numberDepots = 1,
        SteamAppState appState = SteamAppState.StateFullyInstalled);

    SteamAppManifest InstallGame(
        uint id,
        string gameName,
        IList<uint> depots,
        SteamAppState appState = SteamAppState.StateFullyInstalled);


    void InstallCorruptApp();
}