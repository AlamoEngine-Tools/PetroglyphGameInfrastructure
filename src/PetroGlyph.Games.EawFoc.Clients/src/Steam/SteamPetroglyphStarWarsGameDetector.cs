using System;
using System.Diagnostics;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Represents a client for a Petroglyph Star Wars game which allows launching the game.
/// </summary>
public class SteamPetroglyphStarWarsGameClient : PetroglyphStarWarsGameClient
{
    /// <summary>
    /// Returns the Steam wrapper of this instance.
    /// </summary>
    protected readonly ISteamWrapper SteamWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamPetroglyphStarWarsGameClient"/> class with the specified game.
    /// </summary>
    /// <param name="game">The game of the client.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The platform of <paramref name="game"/> is not <see cref="GamePlatform.SteamGold"/>.</exception>
    public SteamPetroglyphStarWarsGameClient(IGame game, IServiceProvider serviceProvider) : base(game, serviceProvider)
    {
        if (game.Platform is not GamePlatform.SteamGold)
            throw new ArgumentException($"The game's platform is not '{GamePlatform.SteamGold}'", nameof(game));
        SteamWrapper = serviceProvider.GetRequiredService<ISteamWrapperFactory>().CreateWrapper();
    }

    /// <inheritdoc />
    protected override void OnGameStarting(IArgumentCollection arguments, GameBuildType type)
    {
        if (!SteamWrapper.IsRunning)
            throw new GameStartException(Game, $"Cannot start game {Game} because Steam Client is not running.");
        base.OnGameStarting(arguments, type);
    }

    /// <inheritdoc />
    protected override void DisposeManagedResources()
    {
        SteamWrapper.Dispose();
        base.DisposeManagedResources();
    }
}

/// <summary>
/// Provides initialization routines for this library.
/// </summary>
public class SteamPetroglyphStarWarsGameClients
{
    /// <summary>
    /// Adds services provided by this library to the specified <paramref name="serviceCollection"/>
    /// so that the library can be used in client applications. 
    /// </summary>
    /// <param name="serviceCollection">The service collection to be filled.</param>
    public static void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGameClientFactory>(sp => new SteamGameClientsFactory(sp));
    }
}

internal sealed class SteamGameClientsFactory(IServiceProvider serviceProvider) : IGameClientFactory
{
    public IGameClient CreateClient(IGame game)
    {
        return game.Platform is GamePlatform.SteamGold
            ? new SteamPetroglyphStarWarsGameClient(game, serviceProvider)
            : new PetroglyphStarWarsGameClient(game, serviceProvider);
    }
}


/// <summary>
/// A <see cref="IGameDetector"/> that is able to find game installations from Steam.
/// </summary>
/// <remarks>
/// This detector supports game initialization requests.
/// </remarks>
public sealed class SteamPetroglyphStarWarsGameDetector : GameDetectorBase
{
    private const uint EaWGameId = 32470;
    private const uint FocDepotId = 32472;

    private readonly ISteamWrapperFactory _steamWrapperFactory;
    private readonly IGameRegistryFactory _registryFactory;

    /// <summary>
    /// Creates a new instance.
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