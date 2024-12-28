using System;
using System.IO.Abstractions;
using AET.SteamAbstraction;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Processes;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal class SteamGameLauncher : DefaultGameProcessLauncher
{
    private readonly ISteamWrapper _steamWrapper;

    public SteamGameLauncher(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        _steamWrapper = serviceProvider.GetRequiredService<ISteamWrapperFactory>().CreateWrapper();
    }

    public override IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        if (!_steamWrapper.IsRunning)
            throw new GameStartException(processInfo.Game, "Unable to start the game, because Steam is not running.");
        return base.StartGameProcess(executable, processInfo);
    }
}