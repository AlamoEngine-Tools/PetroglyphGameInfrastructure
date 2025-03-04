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
/// <remarks>
/// Creates a new instance.
/// </remarks>
/// <param name="serviceProvider">Service Provider</param>
internal sealed class GamePlatformIdentifier(IServiceProvider serviceProvider) : IGamePlatformIdentifier
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GamePlatformIdentifier));

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

    /// <inheritdoc/>
    public GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));
        foreach (var platform in DefaultGamePlatformOrdering)
        {
            var validator = GamePlatformIdentifierFactory.Create(platform, _serviceProvider);
            _logger?.LogTrace($"Validating location for {platform}...");
            if (!validator.IsPlatform(type, ref location))
                continue;

            _logger?.LogTrace($"Game location was identified as {platform}.");
            return platform;
        }

        _logger?.LogTrace("Unable to determine which which platform the game has.");
        return GamePlatform.Undefined;
    }
}