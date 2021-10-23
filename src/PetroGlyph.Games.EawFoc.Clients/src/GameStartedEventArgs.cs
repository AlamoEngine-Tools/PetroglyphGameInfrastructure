using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

public class GameStartedEventArgs : EventArgs
{
    public IGame Game { get; }

    public IReadOnlyCollection<IGameArgument> GameArguments { get; }

    public GameBuildType BuildType { get; }

    public System.Diagnostics.Process Process { get; }

    public GameStartedEventArgs(IGame game, IReadOnlyCollection<IGameArgument> gameArguments, GameBuildType buildType, System.Diagnostics.Process process)
    {
        Game = game;
        GameArguments = gameArguments;
        BuildType = buildType;
        Process = process;
    }
}