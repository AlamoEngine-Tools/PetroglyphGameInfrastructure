using System.Diagnostics;

namespace AET.SteamAbstraction.Utilities;

internal class ProcessHelper : IProcessHelper
{
    public bool IsProcessRunning(int pid)
    {
        try
        {
            Process.GetProcessById(pid);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Process? StartProcess(ProcessStartInfo startInfo)
    {
        return Process.Start(startInfo);
    }
}