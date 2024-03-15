using System;

namespace AET.SteamAbstraction;

internal interface ISteamWrapperFactory
{
    ISteamWrapper CreateWrapper(IServiceProvider serviceProvider);
}