using System;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace PG.StarWarsGame.Infrastructure.Games.Registry;

/// <inheritdoc cref="IGameRegistryFactory"/>
internal class GameRegistryFactory : IGameRegistryFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc cref="IGameRegistryFactory"/>
    public GameRegistryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    internal const string FocRegistryPath =
        @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption";

    internal const string EawRegistryPath =
        @"SOFTWARE\LucasArts\Star Wars Empire at War";

    /// <inheritdoc/>
    public IGameRegistry CreateRegistry(GameType type)
    {
        var registry = _serviceProvider.GetRequiredService<IRegistry>();

        var baseKey = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

        var gamePath = type switch
        {
            GameType.Eaw => EawRegistryPath,
            GameType.Foc => FocRegistryPath,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return new GameRegistry(type, baseKey, gamePath, _serviceProvider);
    }
}