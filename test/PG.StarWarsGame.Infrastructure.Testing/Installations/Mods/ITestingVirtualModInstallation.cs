using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

public interface ITestingVirtualModInstallation : ITestingModInstallation
{
    new IVirtualMod Mod { get; }
}