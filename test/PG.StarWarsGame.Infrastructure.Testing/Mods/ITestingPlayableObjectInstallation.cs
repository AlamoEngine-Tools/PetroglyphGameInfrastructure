using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

public interface ITestingPlayableObjectInstallation
{
    ITestingGameInstallation GameInstallation { get; }

    IPlayableObject PlayableObject { get; }
}