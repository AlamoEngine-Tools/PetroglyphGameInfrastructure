using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

internal interface IGameExecutableNameBuilder
{
    IReadOnlyCollection<GamePlatform> SupportedPlatforms { get; }

    string GetExecutableFileName(IGame game, GameBuildType buildType);
}