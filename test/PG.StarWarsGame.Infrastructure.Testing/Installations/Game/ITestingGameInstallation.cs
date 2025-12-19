using PG.StarWarsGame.Infrastructure.Games;
using System.IO.Abstractions;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

public interface ITestingGameInstallation : ITestingModContainerInstallation
{
    IGame Game { get; }
    
    void InstallDebug();

    IDirectoryInfo GetWrongOriginFocRegistryLocation();

    ITestingPhysicalModInstallation InstallMod(string name);

    ITestingPhysicalModInstallation InstallMod(string name, bool workshop);

    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo);

    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, bool workshop);

    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, IDirectoryInfo directory);

    ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, IDirectoryInfo directory, bool workshop);


    ITestingPhysicalModInstallation InstallAndAddMod(string name);

    ITestingPhysicalModInstallation InstallAndAddMod(string name, bool workshop);

    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo);

    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, bool workshop);

    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, IDirectoryInfo directory);

    ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, IDirectoryInfo directory, bool workshops);

    ITestingPhysicalModInstallation InstallAndAddMod(string name, bool isWorkshop, IModDependencyList dependencies);

    ITestingPhysicalModInstallation InstallAndAddMod(string name, IModDependencyList dependencies);



    ITestingVirtualModInstallation AddVirtualMod(string name, ModinfoData modinfo);
}