using System;
using System.Diagnostics;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// A game detector with extended Steam support which supports game initialization requests.
/// </summary>
public sealed class SteamPetroglyphStarWarsGameDetector : GameDetectorBase
{
    private const uint EaWGameId = 32470;
    private const uint FocDepotId = 32472;

    private readonly ISteamWrapperFactory _steamWrapperFactory;
    private readonly IGameRegistryFactory _registryFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamPetroglyphStarWarsGameDetector"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public SteamPetroglyphStarWarsGameDetector(IServiceProvider serviceProvider) : base(serviceProvider, true)
    {
        _registryFactory = ServiceProvider.GetRequiredService<IGameRegistryFactory>();
        _steamWrapperFactory = ServiceProvider.GetRequiredService<ISteamWrapperFactory>();
    }

    /// <inheritdoc />
    protected override GameLocationData FindGameLocation(GameType gameType)
    {
        using var steam = _steamWrapperFactory.CreateWrapper();

        if (!steam.Installed)
            return GameLocationData.NotInstalled;

        if (!steam.IsGameInstalled(EaWGameId, out var game))
            return GameLocationData.NotInstalled;

        if (!game.State.HasFlag(SteamAppState.StateFullyInstalled))
            return GameLocationData.NotInstalled;

        if (gameType == GameType.Foc && !game.Depots.Contains(FocDepotId))
            return GameLocationData.NotInstalled;

        // This only contains the root directory
        var gameLocation = game.InstallDir;
        var fullGamePath = gameLocation.FullName;
        fullGamePath = gameType switch
        {
            GameType.Foc => FileSystem.Path.Combine(fullGamePath, "corruption"),
            GameType.Eaw => FileSystem.Path.Combine(fullGamePath, "GameData"),
            _ => fullGamePath
        };

        var installLocation = FileSystem.DirectoryInfo.New(fullGamePath);
        if (!GameExeExists(installLocation, gameType))
            return GameLocationData.NotInstalled;

        using var registry = _registryFactory.CreateRegistry(gameType);
        Debug.Assert(registry.Type == gameType);
        if (registry.Version is null)
        {
            Logger?.LogDebug("Registry-Key found, but games are not initialized.");
            return GameLocationData.RequiresInitialization;
        }

        return new GameLocationData(installLocation);
    }
}