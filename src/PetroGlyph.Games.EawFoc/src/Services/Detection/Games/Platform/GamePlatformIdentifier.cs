using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Services.Detection.Platform;

/// <summary>
/// Default implementation of the <see cref="IGamePlatformIdentifier"/> service.
/// </summary>
internal sealed class GamePlatformIdentifier : IGamePlatformIdentifier
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;

    /// <summary>
    /// Default ordering of <see cref="GamePlatform"/>s for identification.
    /// </summary>
    public static readonly IList<GamePlatform> DefaultGamePlatformOrdering =
    [
        // Sorted by (guessed) number of user installations
        GamePlatform.SteamGold,
        GamePlatform.GoG,
        GamePlatform.Origin,

        // Disk Platforms have the lowest priority, because their heuristics are less precise/specific than the above platforms. 
        // This could lead to false positives. (E.g.: Game is "Steam", but detection was "DiskGold")
        GamePlatform.DiskGold,
        GamePlatform.Disk
    ];

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">Service Provider</param>
    public GamePlatformIdentifier(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GamePlatformIdentifier));
    }

    /// <inheritdoc/>
    public GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location, IList<GamePlatform> lookupPlatforms)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));
        if (lookupPlatforms == null)
            throw new ArgumentNullException(nameof(lookupPlatforms));
        return GetGamePlatformCore(type, ref location, NormalizeLookupPlatforms(lookupPlatforms));
    }

    private GamePlatform GetGamePlatformCore(GameType type, ref IDirectoryInfo location, IEnumerable<GamePlatform> lookupPlatforms)
    {
        _logger?.LogDebug("Validating game platform:");
        foreach (var platform in lookupPlatforms)
        {
            var validator = GamePlatformIdentifierFactory.Create(platform, _serviceProvider);
            _logger?.LogDebug($"Validating location for {platform}...");
            if (!validator.IsPlatform(type, ref location))
                continue;

            _logger?.LogDebug($"Game location was identified as {platform}");
            return platform;
        }

        _logger?.LogDebug("Unable to determine which which platform the game has.");
        return GamePlatform.Undefined;
    }

    private IList<GamePlatform> NormalizeLookupPlatforms(IList<GamePlatform> lookupPlatforms)
    {
        return lookupPlatforms.Contains(GamePlatform.Undefined) ? DefaultGamePlatformOrdering : lookupPlatforms;
    }
}