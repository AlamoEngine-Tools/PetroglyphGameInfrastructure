using System;
using System.Globalization;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;

namespace PG.StarWarsGame.Infrastructure.Services;

/// <inheritdoc/>
public sealed class GameFactory : IGameFactory
{
    private static readonly IGameNameResolver FallbackNameResolver = new EnglishGameNameResolver();
    private readonly IGameNameResolver _nameResolver;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Creates a new factory instances. Uses <see cref="EnglishGameNameResolver"/> for the game's name resolution.
    /// </summary>
    /// <param name="serviceProvider">The service provider which gets passed to the game instances.</param>
    public GameFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _nameResolver = serviceProvider.GetService<IGameNameResolver>() ?? FallbackNameResolver;
    }
    
    /// <inheritdoc/>
    public IGame CreateGame(GameDetectionResult gameDetection, CultureInfo culture)
    {
        if (gameDetection == null)
            throw new ArgumentNullException(nameof(gameDetection));
        if (gameDetection.Error is not null)
            throw new GameException("Unable to create a game from faulted detection result.");
        if (gameDetection.GameLocation is null)
            throw new GameException($"Unable to create game {gameDetection.GameIdentity.Type}, because it's not installed on this machine");
        return CreateGame(gameDetection.GameIdentity, gameDetection.GameLocation, false, culture);
    }

    /// <inheritdoc/>
    public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture)
    {
        if (location == null) 
            throw new ArgumentNullException(nameof(location));
        if (identity.Platform == GamePlatform.Undefined)
            throw new ArgumentException("Cannot create a game with undefined platform");

        var name = _nameResolver.ResolveName(identity, culture);
        var game = new PetroglyphStarWarsGame(identity, location, name, _serviceProvider);
        if (checkGameExists && !game.Exists())
            throw new GameException($"Game does not exists at location: {location}");

        return game;
    }

    /// <inheritdoc/>
    public bool TryCreateGame(GameDetectionResult gameDetection, CultureInfo cultureInfo, out IGame? game)
    {
        game = null;
        try
        {
            game = CreateGame(gameDetection, cultureInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture, out IGame? game)
    {
        game = null;
        try
        {
            game = CreateGame(identity, location, checkGameExists, culture);
            return true;
        }
        catch
        {
            return false;
        }
    }
}