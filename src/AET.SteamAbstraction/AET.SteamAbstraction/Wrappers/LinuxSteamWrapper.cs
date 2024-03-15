﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace AET.SteamAbstraction;

internal class LinuxSteamWrapper(IServiceProvider serviceProvider) : SteamWrapper(serviceProvider)
{
    protected override Task WaitSteamUserLoggedInAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    protected override Task WaitSteamRunningAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}