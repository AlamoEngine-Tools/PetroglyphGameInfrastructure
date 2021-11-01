using System;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

internal class DefaultGameProcessLauncher : IGameProcessLauncher
{
    public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        throw new NotImplementedException();
    }
}