using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

public interface ITestingPhysicalModInstallation : ITestingModInstallation, ITestingPhysicalPlayableObjectInstallation
{
    new IPhysicalMod Mod { get; }

    IModinfoFile InstallInvalidModinfoFile(string? variantSubFileName = null);

    IModinfoFile InstallModinfoFile(IModinfo modinfo, string? variantSubFileName = null);
}