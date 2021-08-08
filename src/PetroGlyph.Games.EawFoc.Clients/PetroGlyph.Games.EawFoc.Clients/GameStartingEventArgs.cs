using System.Collections.Generic;
using System.ComponentModel;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients
{
    public class GameStartingEventArgs : CancelEventArgs
    {
        public IGame Game { get; }

        public IReadOnlyCollection<IGameArgument> GameArguments { get; }

        public GameBuildType BuildType { get; }

        public GameStartingEventArgs(IGame game, IReadOnlyCollection<IGameArgument> arguments, GameBuildType buildType)
        {
            Game = game;
            GameArguments = arguments;
            BuildType = buildType;
        }
    }
}