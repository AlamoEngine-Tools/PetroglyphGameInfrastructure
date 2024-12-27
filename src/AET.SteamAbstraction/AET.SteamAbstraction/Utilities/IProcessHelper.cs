using System.Diagnostics;

namespace AET.SteamAbstraction.Utilities;

internal interface IProcessHelper
{
    bool IsProcessRunning(int pid);

    Process? StartProcess(ProcessStartInfo startInfo);
}