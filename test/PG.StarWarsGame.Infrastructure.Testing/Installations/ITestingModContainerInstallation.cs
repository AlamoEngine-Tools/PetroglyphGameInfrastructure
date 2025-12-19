namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

public interface ITestingModContainerInstallation : ITestingPlayableObjectInstallation
{
    PlayableModContainer ModContainer { get; }
}