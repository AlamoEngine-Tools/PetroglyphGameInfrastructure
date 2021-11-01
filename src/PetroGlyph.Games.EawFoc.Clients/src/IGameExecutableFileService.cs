using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Clients;

public interface IGameExecutableFileService
{
    IFileInfo? GetExecutableForGame(IGame game, GameBuildType buildType);
}