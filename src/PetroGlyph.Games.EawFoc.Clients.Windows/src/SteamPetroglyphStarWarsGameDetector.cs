using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Registry;
using PetroGlyph.Games.EawFoc.Services.Detection;

namespace PetroGlyph.Games.EawFoc.Clients;

public class SteamPetroglyphStarWarsGameDetector : GameDetector
{
    private const uint EaWGameId = 32470;
    private const uint FocDepotId = 32472;

    private readonly ISteamWrapper _steamWrapper;
    private readonly IGameRegistryFactory _registryFactory;

    public SteamPetroglyphStarWarsGameDetector(IServiceProvider serviceProvider) : base(serviceProvider, true)
    {
        _registryFactory = ServiceProvider.GetRequiredService<IGameRegistryFactory>();
        _steamWrapper = ServiceProvider.GetRequiredService<ISteamWrapper>();
    }

    protected override GameLocationData FindGameLocation(GameDetectorOptions options)
    { 
        if (!_steamWrapper.Installed)
            return default;

        if (!_steamWrapper.IsGameInstalled(EaWGameId, out var game))
            return default;

        if (!game.State.HasFlag(SteamAppState.StateFullyInstalled))
            return default;

        if (options.Type == GameType.Foc && !game.Depots.Contains(FocDepotId))
            return default;

        // This only contains the root directory
        var gameLocation = game.InstallDir;
        var fullGamePath = gameLocation.FullName;
        fullGamePath = options.Type switch
        {
            GameType.Foc => FileSystem.Path.Combine(fullGamePath, "corruption"),
            GameType.EaW => FileSystem.Path.Combine(fullGamePath, "GameData"),
            _ => fullGamePath
        };

        var installLocation = FileSystem.DirectoryInfo.FromDirectoryName(fullGamePath);

        using var registry = _registryFactory.CreateRegistry(options.Type, ServiceProvider);
        if (registry.Type != options.Type)
            throw new InvalidOperationException("Incompatible registry");

        if (registry.Version is null)
        {
            Logger?.LogDebug("Registry-Key found, but games are not initialized.");
            return new GameLocationData { InitializationRequired = true };
        }

        return !GameExeExists(installLocation, options.Type)
            ? default
            : new GameLocationData { Location = installLocation };
    }
}