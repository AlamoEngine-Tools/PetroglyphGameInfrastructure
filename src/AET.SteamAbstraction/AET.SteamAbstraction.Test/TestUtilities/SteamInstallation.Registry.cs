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

    private static void InstallLinuxRegistry(LinuxSteamRegistry registry, IFileSystem fs)
    {
        // TODO
    }

    private static void InstallWindowsRegistry(WindowsSteamRegistry registry, IFileSystem fs)
    {
        using var key = registry.GetSteamRegistryKey();

        key.SetValue("SteamExe", fs.Path.GetFullPath(SteamExePath));
        key.SetValue("SteamPath", fs.Path.GetFullPath(SteamInstallPath));
    }
}