using System.IO.Abstractions;
using AET.SteamAbstraction.Registry;

namespace AET.SteamAbstraction.Test.TestUtilities;

internal static partial class SteamInstallation
{
    private const string SteamInstallPath = "steam";
    private const string SteamExePath = "steam/steam.exe";

    public static void InstallSteam(this IFileSystem fs, ISteamRegistry registry)
    {
        registry.InstallSteam(fs);
        fs.InstallSteamFiles();
    }

    public static void InstallSteamFiles(this IFileSystem fs)
    {
        var configPath = fs.Path.GetFullPath(fs.Path.Combine(SteamInstallPath, "config"));
        var exePath = fs.Path.GetFullPath(SteamExePath);

        fs.Directory.CreateDirectory(configPath);
        using var _ = fs.File.Create(exePath);
    }
}