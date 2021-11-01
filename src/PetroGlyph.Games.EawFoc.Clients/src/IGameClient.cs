using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

public interface IGameClient
{
    event EventHandler<IGameProcess> GameStarted;

    event EventHandler<GameStartingEventArgs> GameStarting;

    event EventHandler<IGameProcess> GameClosed;

    IGameArgumentCollection DefaultArguments { get; }

    IReadOnlyCollection<IPlayableObject> RunningInstances { get; }

    ISet<GamePlatform> SupportedPlatforms { get; }

    IGameProcess Play(IPlayableObject instance);

    IGameProcess Play(IPlayableObject instance, IGameArgumentCollection arguments);
}