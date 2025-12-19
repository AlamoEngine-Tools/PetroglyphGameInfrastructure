using System;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

internal sealed class TestingVirtualModImpl(ITestingGameInstallation gameInstallation, IVirtualMod mod, IServiceProvider serviceProvider)
    : TestingModImpl(gameInstallation, mod, serviceProvider), ITestingVirtualModInstallation
{
    public ITestingGameInstallation GameInstallation { get; } = gameInstallation;

    public IVirtualMod Mod { get; } = mod;
}