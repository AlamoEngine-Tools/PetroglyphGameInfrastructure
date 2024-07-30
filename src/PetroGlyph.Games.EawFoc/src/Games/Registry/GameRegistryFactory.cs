using System;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Infrastructure.Games.Registry;

/// <inheritdoc cref="IGameRegistryFactory"/>
public class GameRegistryFactory(IServiceProvider serviceProvider) : IGameRegistryFactory
{
    internal const string FocRegistryPath =
        @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption";

    internal const string EawRegistryPath =
        @"SOFTWARE\LucasArts\Star Wars Empire at War";

    /// <inheritdoc/>
    public IGameRegistry CreateRegistry(GameType type)
    {
        if (serviceProvider == null) 
            throw new ArgumentNullException(nameof(serviceProvider));
        var registry = serviceProvider.GetRequiredService<IRegistry>();
        var baseKey = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

        var gamePath = type switch
        {
            GameType.Eaw => EawRegistryPath,
            GameType.Foc => FocRegistryPath,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var gameKey = baseKey.GetKey(gamePath);
        if (gameKey is null)
            throw new GameRegistryNotFoundException();
        return new GameRegistry(type, gameKey, serviceProvider);
    }
}