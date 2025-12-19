using System;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

internal sealed class TestingPhysicalModImpl(ITestingGameInstallation gameInstallation, IPhysicalMod mod, IServiceProvider serviceProvider) 
    : TestingModImpl(gameInstallation, mod, serviceProvider), ITestingPhysicalModInstallation
{
    public ITestingGameInstallation GameInstallation { get; } = gameInstallation;
    
    public IPhysicalMod Mod { get; } = mod;
}