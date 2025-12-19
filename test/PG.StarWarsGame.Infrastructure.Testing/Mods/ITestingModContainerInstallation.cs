namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

public interface ITestingModContainerInstallation : ITestingPlayableObjectInstallation
{
    PlayableModContainer ModContainer { get; }
}