using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

/// <summary>
/// Metadata for an <see cref="IGameProcess"/>.
/// Also holds  the set of values that are used when the game is started.
/// </summary>
public class GameProcessInfo
{
    /// <summary>
    /// The requested <see cref="GameBuildType"/>.
    /// </summary>
    public GameBuildType BuildType { get; }

    /// <summary>
    /// The played instance.
    /// </summary>
    public IPlayableObject PlayedInstance { get; }

    /// <summary>
    /// The arguments of the game process.
    /// </summary>
    public IArgumentCollection Arguments { get; }

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="playedInstance">The played instance.</param>
    /// <param name="buildType">The requested <see cref="GameBuildType"/>.</param>
    /// <param name="arguments">The arguments of the game process.</param>
    public GameProcessInfo(IPlayableObject playedInstance, GameBuildType buildType, IArgumentCollection arguments)
    {
        Requires.NotNull(playedInstance, nameof(playedInstance));
        Requires.NotNull(arguments, nameof(arguments));
        PlayedInstance = playedInstance;
        BuildType = buildType;
        Arguments = arguments;
    }
}