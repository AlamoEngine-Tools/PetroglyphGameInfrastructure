using PG.StarWarsGame.Infrastructure.Testing.Installations.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

public interface ITestingPlayableObjectInstallation
{
    ITestingGameInstallation GameInstallation { get; }

    IPlayableObject PlayableObject { get; }
}