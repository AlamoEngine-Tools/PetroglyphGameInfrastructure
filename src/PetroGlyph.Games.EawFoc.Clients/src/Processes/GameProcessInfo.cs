using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

public class GameProcessInfo
{
    public GameBuildType BuildType { get; }

    public IPlayableObject PlayedInstance { get; }

    public IArgumentCollection Arguments { get; }

    public GameProcessInfo(IPlayableObject playedInstance, GameBuildType buildType, IArgumentCollection arguments)
    {
        Requires.NotNull(playedInstance, nameof(playedInstance));
        Requires.NotNull(arguments, nameof(arguments));
        PlayedInstance = playedInstance;
        BuildType = buildType;
        Arguments = arguments;
    }
}