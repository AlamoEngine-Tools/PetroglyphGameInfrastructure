using System;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Registry;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

/// <summary>
/// Finds installed games from the registry.
/// </summary>
public sealed class RegistryGameDetector : GameDetector, IDisposable
{
    private readonly IGameRegistry _eawRegistry;
    private readonly IGameRegistry _focRegistry;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="eawRegistry">The registry instance for Empire at War.</param>
    /// <param name="focRegistry">The registry instance for Forces of Corruption.</param>
    /// <param name="tryHandleInitialization">
    /// Indicates whether this instance shall raise the <see cref="GameDetector.InitializationRequested"/>event.
    /// When set to <see langword="false"/> the event will not be raised and initialization cannot be handled.</param>
    public RegistryGameDetector(IGameRegistry eawRegistry, IGameRegistry focRegistry, bool tryHandleInitialization, IServiceProvider serviceProvider)
        : base(serviceProvider, tryHandleInitialization)
    {
        Requires.NotNull(eawRegistry, nameof(eawRegistry));
        Requires.NotNull(focRegistry, nameof(focRegistry));
        _eawRegistry = eawRegistry;
        _focRegistry = focRegistry;
    }

    /// <inheritdoc/>
    protected internal override GameLocationData FindGameLocation(GameDetectorOptions options)
    {
        Logger?.LogDebug("Attempting to fetch the game from the registry.");
        var registry = GetRegistry(options);
        if (!registry.Exits)
        {
            Logger?.LogDebug("The Game's Registry does not exist.");
            return default;
        }

        if (registry.Version is null)
        {
            Logger?.LogDebug("Registry-Key found, but games are not initialized.");
            return new GameLocationData { InitializationRequired = true };
        }

        var exeDirectory = registry.ExePath?.Directory;
        if (exeDirectory is not null)
            return new GameLocationData { Location = exeDirectory };

        Logger?.LogDebug("Could not get instal location from registry path.");
        return default;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _eawRegistry.Dispose();
        _focRegistry.Dispose();
    }

    private IGameRegistry GetRegistry(GameDetectorOptions options)
    {
        return options.Type == GameType.Foc ? _focRegistry : _eawRegistry;
    }
}