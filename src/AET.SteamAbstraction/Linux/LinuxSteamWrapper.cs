using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace AET.SteamAbstraction;

[ExcludeFromCodeCoverage]
internal class LinuxSteamWrapper(LinuxSteamRegistry registry, IServiceProvider serviceProvider) : SteamWrapper(registry, serviceProvider)
{
    public override bool IsRunning => throw new NotImplementedException();

    protected override void ResetCurrentUser()
    {
        throw new NotImplementedException();
    }

    protected internal override uint GetCurrentUserId()
    {
        throw new NotImplementedException();
    }

    protected internal override Task WaitSteamOfflineRunning(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    protected internal override Task WaitSteamUserLoggedInAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    protected internal override Task WaitSteamRunningAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}