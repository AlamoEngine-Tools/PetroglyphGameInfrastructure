using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients;

public class SteamGameClient : DebugableClientBase
{
    public SteamGameClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Starts the game bound to the given <paramref name="instance"/>.
    /// If <paramref name="instance"/> represents an <see cref="IMod"/> the mod's dependencies get added to the launch arguments.
    /// </summary>
    /// <param name="instance">The game or mod to play.</param>
    /// <returns>The game's process.</returns>
    public override IGameProcess Play(IPlayableObject instance)
    {
        var arguments = DefaultArguments;
        if (instance is IMod mod)
        {
            var argFactory = ServiceProvider.GetService<IModArgumentListFactory>() ?? new ModArgumentListFactory(ServiceProvider);
            var modArgs = argFactory.BuildArgumentList(mod);
            arguments = ArgumentCollection.Merge(DefaultArguments, modArgs);
        }
        return Play(instance, arguments);
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