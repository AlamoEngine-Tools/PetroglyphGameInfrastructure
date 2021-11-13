using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

/// <summary>
/// <see cref="IDebugableGameClient"/> dedicated to the steam version of the games.
/// </summary>
public sealed class SteamGameClient : DebugableClientBase
{
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SteamGameClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Returns a custom <see cref="IGameProcessLauncher"/> which checks if Steam is running.
    /// </summary>
    /// <returns>The game launcher.</returns>
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
            _internalLauncher = serviceProvider.GetService<IGameProcessLauncher>() ?? new DefaultGameProcessLauncher(serviceProvider);
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