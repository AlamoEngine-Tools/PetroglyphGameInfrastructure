using System;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

internal class TestingModImpl(ITestingGameInstallation gameInstallation, IMod mod, IServiceProvider serviceProvider) : ITestingModInstallation
{
    public ITestingGameInstallation GameInstallation { get; } = gameInstallation;

    public IPlayableObject PlayableObject => Mod;
    
    public IMod Mod { get; } = mod;

    public PlayableModContainer ModContainer => Mod as ModBase;
}