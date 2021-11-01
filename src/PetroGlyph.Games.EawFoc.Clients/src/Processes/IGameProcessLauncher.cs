using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

public interface IGameProcessLauncher
{
    IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo);
}