using System;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Registry;
using Validation;

namespace PetroGlyph.Games.EawFoc.Games.Registry;

/// <inheritdoc cref="IGameRegistryFactory"/>
public class GameRegistryFactory : IGameRegistryFactory
{
    private const string FocRegistryPath =
        @"SOFTWARE\LucasArts\Star Wars Empire at War Forces of Corruption";

    private const string EawRegistryPath =
        @"SOFTWARE\LucasArts\Star Wars Empire at War";

    /// <inheritdoc/>
    public IGameRegistry CreateRegistry(GameType type, IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        var registry = serviceProvider.GetRequiredService<IRegistry>();
        var baseKey = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

        var gamePath = type switch
        {
            GameType.EaW => EawRegistryPath,
            GameType.Foc => FocRegistryPath,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var gameKey = baseKey.GetKey(gamePath);
        if (gameKey is null)
            throw new InvalidOperationException($"Unable to get registry key for game {type}");
        return new GameRegistry(type, gameKey, serviceProvider);
    }
}