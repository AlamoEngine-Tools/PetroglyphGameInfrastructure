using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

internal abstract class TestingPlayableObjectInstallationImpl(IServiceProvider serviceProvider) 
    : ITestingPlayableObjectInstallation
{
    protected readonly IServiceProvider ServiceProvider = serviceProvider;
    protected readonly IFileSystem FileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public abstract ITestingGameInstallation GameInstallation { get; }

    public abstract IPlayableObject PlayableObject { get; }
}