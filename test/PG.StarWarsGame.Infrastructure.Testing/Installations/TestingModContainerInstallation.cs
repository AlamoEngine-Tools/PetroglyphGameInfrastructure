using System;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

internal abstract class TestingModContainerInstallation(IServiceProvider serviceProvider)
    : TestingPlayableObjectInstallationImpl(serviceProvider), ITestingModContainerInstallation
{
    public abstract PlayableModContainer ModContainer { get; }
}