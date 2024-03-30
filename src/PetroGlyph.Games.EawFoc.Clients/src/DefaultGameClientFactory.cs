using System;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// <see cref="IGameClientFactory"/> which creates a <see cref="SteamGameClient"/> for the Steam platform
/// and <see cref="DefaultClient"/> otherwise.
/// <para>
/// If this factory already created an instance for a matching <see cref="GamePlatform"/> it will reuse the instance. 
/// </para>
/// </summary>
public class DefaultGameClientFactory : IGameClientFactory
{
    private readonly IServiceProvider _serviceProvider;

    private IGameClient? _steamClient;
    private IGameClient? _normalClient;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DefaultGameClientFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets or creates an <see cref="IGameClient"/> for the given <paramref name="gamePlatform"/>.
    /// </summary>
    /// <param name="gamePlatform">The requested game platform.</param>
    /// <param name="serviceProvider">The service provider used to create the <see cref="IGameClient"/>.</param>
    /// <returns></returns>
    public IGameClient CreateClient(GamePlatform gamePlatform, IServiceProvider serviceProvider)
    {
        if (gamePlatform == GamePlatform.SteamGold)
        {
            _steamClient ??= new SteamGameClient(_serviceProvider);
            return _steamClient;
        }
        _normalClient ??= new DefaultClient(_serviceProvider);
        return _normalClient;
    }
}