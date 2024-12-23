using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Registry;

namespace AET.SteamAbstraction.Registry;

internal interface IWindowsSteamRegistry : ISteamRegistry
{
    IRegistryKey? ActiveProcessKey { get; }
    
    ISet<uint>? InstalledApps { get; }

    int? ActiveUserId { get; set; }
}