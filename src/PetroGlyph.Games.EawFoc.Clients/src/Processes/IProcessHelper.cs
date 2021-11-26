using System.Diagnostics;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

internal interface IProcessHelper
{
    Process? GetProcessByPid(int pid);

    Process? FindProcess(string name);
}