using PG.StarWarsGame.Infrastructure.Games;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Testing.Mods;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

public interface ITestingGameInstallation
{
    IGame Game { get; }
    
    void InstallDebug();

    IDirectoryInfo GetWrongOriginFocRegistryLocation();

    ITestingModInstallation InstallAndAddMod(string name, bool workshop);
}