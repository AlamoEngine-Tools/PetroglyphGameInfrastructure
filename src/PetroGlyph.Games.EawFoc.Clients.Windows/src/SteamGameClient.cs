using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients;

public class SteamGameClient : DebugableClientBase
{
    public SteamGameClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override IGameProcessLauncher GetGameLauncherService()
    {
        return new SteamGameLauncher(ServiceProvider);
    }

    private class SteamGameLauncher : IGameProcessLauncher
    {
        private readonly ISteamWrapper _steamWrapper;
        private readonly IGameProcessLauncher _internalLauncher;

        public SteamGameLauncher(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _steamWrapper = serviceProvider.GetRequiredService<ISteamWrapper>();
            _internalLauncher = serviceProvider.GetService<IGameProcessLauncher>() ?? new DefaultGameProcessLauncher();
        }

        public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
        {
            if (!_steamWrapper.IsRunning)
                throw new GameStartException(processInfo.PlayedInstance,
                    "Unable to start the game, because Steam is not running.");
            return _internalLauncher.StartGameProcess(executable, processInfo);
        }
    }
}