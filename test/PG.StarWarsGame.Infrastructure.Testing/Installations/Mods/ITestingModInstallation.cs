using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

public interface ITestingModInstallation : ITestingModContainerInstallation
{
    new ITestingGameInstallation GameInstallation { get; }

    IMod Mod { get; }
}