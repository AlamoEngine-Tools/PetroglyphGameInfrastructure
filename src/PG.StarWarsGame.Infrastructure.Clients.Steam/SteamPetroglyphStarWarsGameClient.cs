using System;
using AET.SteamAbstraction;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

/// <summary>
/// Represents a client for a Petroglyph Star Wars game which allows launching the game.
/// </summary>
public class SteamPetroglyphStarWarsGameClient : PetroglyphStarWarsGameClient
{
    /// <summary>
    /// Returns the Steam wrapper of this instance.
    /// </summary>
    protected internal readonly ISteamWrapper SteamWrapper;

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
    protected override void OnGameStarting(ArgumentCollection arguments, GameBuildType type)
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