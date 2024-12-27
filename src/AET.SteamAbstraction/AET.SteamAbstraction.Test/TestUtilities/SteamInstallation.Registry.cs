using System.IO.Abstractions;
using AET.SteamAbstraction.Registry;

namespace AET.SteamAbstraction.Test.TestUtilities;

internal static partial class SteamInstallation
{
    public static void InstallSteam(this ISteamRegistry registry, IFileSystem fs)
    {
        if (registry is WindowsSteamRegistry windowsRegistry)
            InstallWindowsRegistry(windowsRegistry, fs);
        else if (registry is LinuxSteamRegistry linuxRegistry)
            InstallLinuxRegistry(linuxRegistry, fs);
    }
    

    public static void SetPid(this ISteamRegistry registry, int? pid)
    {
        if (registry is WindowsSteamRegistry windowsRegistry)
            SetPidWindowsRegistry(windowsRegistry, pid);
        else if (registry is LinuxSteamRegistry linuxRegistry)
            SetPidLinuxRegistry(linuxRegistry, pid);
    }

    public static void SetUserId(this ISteamRegistry registry, uint userId)
    {
        if (registry is WindowsSteamRegistry windowsRegistry)
            SetUserIdWindowsRegistry(windowsRegistry, userId);
        // Does not exist for Linux
    }
}