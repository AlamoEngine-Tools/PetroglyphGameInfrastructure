using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

public interface ITestingModInstallation : ITestingModContainerInstallation
{
    ITestingGameInstallation GameInstallation { get; }

    IMod Mod { get; }
}