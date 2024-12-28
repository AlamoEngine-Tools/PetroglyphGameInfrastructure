using System;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

/// <summary>
/// Specifies a set of values that are used when starting a game process.
/// </summary>
public sealed class GameProcessInfo
{
    /// <summary>
    /// Gets the requested build type of the game.
    /// </summary>
    public GameBuildType BuildType { get; }

    /// <summary>
    /// Gets the game.
    /// </summary>
    public IGame Game { get; }

    /// <summary>
    /// Gets the set of command-line arguments to use when starting the game.
    /// </summary>
    public IArgumentCollection Arguments { get; }

    internal GameProcessInfo(IGame game, GameBuildType buildType, IArgumentCollection arguments)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
        BuildType = buildType;
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
    }
}