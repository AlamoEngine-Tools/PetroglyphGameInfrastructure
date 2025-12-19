using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

public interface ITestingPhysicalModInstallation : ITestingModInstallation
{
    new IPhysicalMod Mod { get; }
}