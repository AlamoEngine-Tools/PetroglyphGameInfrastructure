using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

public class WindowsGameClientFactory : IGameClientFactory
{
    private readonly IServiceProvider _serviceProvider;

    private IGameClient? _steamClient;
    private IGameClient? _normalClient;

    public WindowsGameClientFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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