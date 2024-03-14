using System;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Base implementation for an <see cref="IDebugableGameClient"/>.
/// </summary>
public abstract class DebugableClientBase : ClientBase, IDebugableGameClient
{
    /// <summary>
    /// Initializes a new instance with a given <paramref name="serviceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    protected DebugableClientBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    public bool IsDebugAvailable(IPlayableObject instance)
    {
        var debugExecutable = ServiceProvider.GetRequiredService<IGameExecutableFileService>()
            .GetExecutableForGame(instance.Game, GameBuildType.Debug);
        return debugExecutable is not null && debugExecutable.Exists;
    }

    /// <inheritdoc/>
    public IGameProcess Debug(IPlayableObject instance)
    {
        return StartGame(instance, GameBuildType.Debug);
    }

    /// <inheritdoc/>
    public IGameProcess Debug(IPlayableObject instance, IArgumentCollection arguments, bool fallbackToPlay)
    {
        var buildType = GameBuildType.Debug;
        if (!IsDebugAvailable(instance))
        {
            if (!fallbackToPlay)
                throw new GameStartException(instance, "Debug version of the game could not be found.");
            buildType = GameBuildType.Release;
        }
        return StartGame(instance, arguments, buildType);
    }
}