using System.IO.Abstractions;

namespace AET.SteamAbstraction.Test.TestUtilities;

internal static partial class SteamInstallation
{
    private static void InstallWindowsRegistry(WindowsSteamRegistry registry, IFileSystem fs)
    {
        using var key = registry.GetSteamRegistryKey();

        key.SetValue("SteamExe", fs.Path.GetFullPath(SteamExePath));
        key.SetValue("SteamPath", fs.Path.GetFullPath(SteamInstallPath));
    }

    private static void SetPidWindowsRegistry(WindowsSteamRegistry registry, int? pid)
    {
        if (pid is null)
            return;
        using var key = registry.GetSteamRegistryKey();
        using var activeProcessKey = key.CreateSubKey("ActiveProcess");

        activeProcessKey!.SetValue("pid", pid);
    }

    private static void SetUserIdWindowsRegistry(WindowsSteamRegistry registry, uint userId)
    {
        using var key = registry.GetSteamRegistryKey();
        using var activeProcessKey = key.CreateSubKey("ActiveProcess");

        activeProcessKey!.SetValue("ActiveUser", userId);
    }
}