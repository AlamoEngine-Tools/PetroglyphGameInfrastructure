using System;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

internal class TestingModInstallationImpl(ITestingGameInstallation gameInstallation, IMod mod, IServiceProvider serviceProvider) 
    : TestingModContainerInstallation(serviceProvider), ITestingModInstallation
{
    public override ITestingGameInstallation GameInstallation { get; } = gameInstallation;

    public override IPlayableObject PlayableObject => Mod;
    
    public IMod Mod { get; } = mod;

    public override PlayableModContainer ModContainer => Mod as ModBase;
}