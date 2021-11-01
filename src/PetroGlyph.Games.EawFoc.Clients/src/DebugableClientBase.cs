using System;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;

namespace PetroGlyph.Games.EawFoc.Clients;

public abstract class DebugableClientBase : ClientBase, IDebugableGameClient
{
    protected DebugableClientBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public bool IsDebugAvailable(IPlayableObject instance)
    {
        var debugExecutable = ServiceProvider.GetRequiredService<IGameExecutableFileService>()
            .GetExecutableForGame(instance.Game, GameBuildType.Debug);
        return debugExecutable is not null && debugExecutable.Exists;
    }

    public IGameProcess Debug(IPlayableObject instance)
    {
        return Debug(instance, DefaultArguments, false);
    }

    public IGameProcess Debug(IPlayableObject instance, IGameArgumentCollection arguments, bool fallbackToPlay)
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