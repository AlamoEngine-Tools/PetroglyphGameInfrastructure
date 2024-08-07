﻿using System;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Services.Detection;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Searches an installation of a Petroglyph Star Wars game on the Steam client.
/// <para>
/// When an installation was found, but was not initialized,
/// this instance will raise an <see cref="IGameDetector.InitializationRequested"/> event.
/// </para>
/// </summary>
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
    protected internal override GameLocationData FindGameLocation(GameDetectorOptions options)
    {
        using var steam = _steamWrapperFactory.CreateWrapper();

        if (!steam.Installed)
            return default;

        if (!steam.IsGameInstalled(EaWGameId, out var game))
            return default;

        if (!game!.State.HasFlag(SteamAppState.StateFullyInstalled))
            return default;

        if (options.Type == GameType.Foc && !game.Depots.Contains(FocDepotId))
            return default;

        // This only contains the root directory
        var gameLocation = game.InstallDir;
        var fullGamePath = gameLocation.FullName;
        fullGamePath = options.Type switch
        {
            GameType.Foc => FileSystem.Path.Combine(fullGamePath, "corruption"),
            GameType.Eaw => FileSystem.Path.Combine(fullGamePath, "GameData"),
            _ => fullGamePath
        };

        var installLocation = FileSystem.DirectoryInfo.New(fullGamePath);

        try
        {
            using var registry = _registryFactory.CreateRegistry(options.Type);
            if (registry.Type != options.Type)
                throw new InvalidOperationException("Incompatible registry");
            if (registry.Version is null)
            {
                Logger?.LogDebug("Registry-Key found, but games are not initialized.");
                return new GameLocationData { InitializationRequired = true };
            }
        }
        catch (GameRegistryNotFoundException)
        {
            Logger?.LogDebug("Registry-Key found, but games are not initialized.");
            return new GameLocationData { InitializationRequired = true };
        }

        return !GameExeExists(installLocation, options.Type)
            ? default
            : new GameLocationData { Location = installLocation };
    }
}