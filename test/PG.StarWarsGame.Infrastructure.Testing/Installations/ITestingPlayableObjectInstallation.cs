using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

public interface ITestingPlayableObjectInstallation
{
    ITestingGameInstallation GameInstallation { get; }

    IPlayableObject PlayableObject { get; }
}

public interface ITestingPhysicalPlayableObjectInstallation : ITestingPlayableObjectInstallation
{
    new IPhysicalPlayableObject PlayableObject { get; }

    void InstallLanguage(ILanguageInfo language);
}