using System;
using PG.StarWarsGame.Infrastructure.Clients.Steam;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal sealed class GameProcessLauncherFactory(IServiceProvider serviceProvider) : IGameProcessLauncherFactory
{
    private readonly DefaultGameProcessLauncher _defaultLauncher = new(serviceProvider);
    private readonly SteamGameLauncher _steamLauncher = new(serviceProvider);

    public IGameProcessLauncher CreateGameProcessLauncher(bool isSteam)
    {
        return isSteam ? _steamLauncher : _defaultLauncher;
    }
}