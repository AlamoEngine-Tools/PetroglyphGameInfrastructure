using System.Diagnostics;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

internal class ProcessHelper : IProcessHelper
{
    public Process? GetProcessByPid(int pid)
    {
        try
        {
            return Process.GetProcessById(pid);
        }
        catch
        {
            return null;
        }
    }

    public Process? FindProcess(string name)
    {
        return Process.GetProcessesByName(name).FirstOrDefault();
    }
}