using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Clients.Arguments;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

public interface IGameProcess
{
    event EventHandler Closed;

    GameBuildType BuildType { get; }

    IPlayableObject PlayedInstance { get; }

    IReadOnlyCollection<IGameArgument> Arguments { get; }

    bool Running { get; }

    bool WasClosed { get; }

    void Exit();
}