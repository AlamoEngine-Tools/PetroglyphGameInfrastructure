using System;
using Microsoft.Extensions.DependencyInjection;

namespace AET.SteamAbstraction.Testing;

internal sealed class SteamFakeProcessImpl : ISteamFakeProcess
{
    private readonly TestProcessHelper _processHelper;

    public SteamFakeProcessImpl(IServiceProvider serviceProvider, int pid)
    {
        _processHelper = serviceProvider.GetRequiredService<TestProcessHelper>();
        _processHelper.SetRunningPid(pid);
    }

    public void Kill()
    {
        _processHelper.KillCurrent();
    }
}