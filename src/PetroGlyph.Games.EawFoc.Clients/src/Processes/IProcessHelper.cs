using System.Diagnostics;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal interface IProcessHelper
{
    Process? GetProcessByPid(int pid);

    Process? FindProcess(string name);
}