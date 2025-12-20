using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.Versioning;

namespace AET.SteamAbstraction.Testing;

[SupportedOSPlatform("windows")]
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