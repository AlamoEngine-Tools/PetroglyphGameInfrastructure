using System.Diagnostics;

namespace AET.SteamAbstraction.Utilities;

internal interface IProcessHelper
{
    Process? GetProcessByPid(int pid);

    Process? FindProcess(string name);
}