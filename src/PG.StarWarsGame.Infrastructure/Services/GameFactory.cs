using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;

namespace PG.StarWarsGame.Infrastructure.Services;

internal sealed class GameFactory(IServiceProvider serviceProvider) : IGameFactory
{
    private readonly IGameNameResolver _nameResolver = serviceProvider.GetRequiredService<IGameNameResolver>();

    private readonly IServiceProvider _serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc/>
    public IGame CreateGame(GameDetectionResult gameDetection, CultureInfo culture)
    {
        if (gameDetection == null)
            throw new ArgumentNullException(nameof(gameDetection));
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        if (!gameDetection.Installed)
            throw new GameException(
                $"Unable to create game {gameDetection.GameIdentity.Type}, because it's not installed on this machine");

        return CreateGame(gameDetection.GameIdentity, gameDetection.GameLocation, false, culture);
    }

    /// <inheritdoc/>
    public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, CultureInfo culture)
    {
        if (identity == null) 
            throw new ArgumentNullException(nameof(identity));
        if (location == null)
            throw new ArgumentNullException(nameof(location));
        if (culture == null)
            throw new ArgumentNullException(nameof(culture));

        if (identity.Platform == GamePlatform.Undefined)
            throw new GameException("Cannot create a game with undefined platform.");

        var name = _nameResolver.ResolveName(identity, culture);
        if (string.IsNullOrEmpty(name))
            throw new GameException("Cannot create game with null or empty name.");

        var game = new PetroglyphStarWarsGame(identity, location, name, _serviceProvider);

        var detector = new DirectoryGameDetector(location, _serviceProvider);
        if (checkGameExists && !detector.Detect(identity.Type, identity.Platform).Installed)
            throw new GameException($"Game does not exists at location: '{location}'.");

        return game;
    }

    /// <inheritdoc/>
    public bool TryCreateGame(GameDetectionResult gameDetection, CultureInfo cultureInfo, [NotNullWhen(true)] out IGame? game)
    {
        game = null;
        try
        {
            game = CreateGame(gameDetection, cultureInfo);
            return true;
        }
        catch (GameException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists,
        CultureInfo culture, [NotNullWhen(true)] out IGame? game)
    {
        game = null;
        try
        {
            game = CreateGame(identity, location, checkGameExists, culture);
            return true;
        }
        catch (GameException)
        {
            return false;
        }
    }
}