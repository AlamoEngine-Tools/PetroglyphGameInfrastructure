using System;
using System.Threading;
using System.Threading.Tasks;
using AET.SteamAbstraction.Registry;

namespace AET.SteamAbstraction;

internal class LinuxSteamWrapper(ISteamRegistry registry, IServiceProvider serviceProvider) : SteamWrapper(registry, serviceProvider)
{
    public override bool IsRunning => throw new NotImplementedException();

    protected override Task WaitSteamOfflineRunning(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    protected override Task WaitSteamUserLoggedInAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    protected override Task WaitSteamRunningAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}