using System;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

/// <summary>
/// A <see cref="IGameDetector"/> that is able to find game installations from the registry.
/// </summary>
/// <remarks>
/// This detector supports game initialization requests.
/// </remarks>
public sealed class RegistryGameDetector : GameDetectorBase, IDisposable
{
    private readonly IGameRegistry _eawRegistry;
    private readonly IGameRegistry _focRegistry;

    /// <summary>
    /// Creates a new instance of the <see cref="RegistryGameDetector"/> class.
    /// </summary>
    /// <param name="eawRegistry">The registry instance for Empire at War.</param>
    /// <param name="focRegistry">The registry instance for Forces of Corruption.</param>
    /// <param name="tryHandleInitialization">
    /// Indicates whether this instance shall raise the <see cref="GameDetectorBase.InitializationRequested"/>event.
    /// When set to <see langword="false"/> the event will not be raised and initialization cannot be handled.
    /// </param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="eawRegistry"/> or <paramref name="focRegistry"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The game type of <paramref name="eawRegistry"/> or <paramref name="focRegistry"/> is not correct.</exception>
    public RegistryGameDetector(IGameRegistry eawRegistry, IGameRegistry focRegistry, bool tryHandleInitialization, IServiceProvider serviceProvider)
        : base(serviceProvider, tryHandleInitialization)
    {
        _eawRegistry = eawRegistry ?? throw new ArgumentNullException(nameof(eawRegistry));
        _focRegistry = focRegistry ?? throw new ArgumentNullException(nameof(focRegistry));

        if (_eawRegistry.Type != GameType.Eaw)
            throw new ArgumentException("Registry for EaW has the wrong game type.", nameof(eawRegistry));
        if (_focRegistry.Type != GameType.Foc)
            throw new ArgumentException("Registry for FoC has the wrong game type.", nameof(eawRegistry));
    }

    /// <inheritdoc/>
    protected override GameLocationData FindGameLocation(GameType gameType)
    {
        Logger?.LogTrace("Attempting to fetch game location from the registry.");
        var registry = GetRegistry(gameType);
        if (!registry.Exits)
        {
            Logger?.LogTrace("The Game's Registry does not exist.");
            return GameLocationData.NotInstalled;
        }

        if (registry.Version is null)
        {
            Logger?.LogTrace("Registry-Key found, but games are not initialized.");
            return GameLocationData.RequiresInitialization;
        }

        var exeDirectory = registry.ExePath?.Directory;
        return new GameLocationData(exeDirectory);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _eawRegistry.Dispose();
        _focRegistry.Dispose();
    }

    private IGameRegistry GetRegistry(GameType gameType)
    {
        return gameType == GameType.Foc ? _focRegistry : _eawRegistry;
    }
}