using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
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

    public static void DeleteLoginUsersFile(this IFileSystem fs)
    {
        var configPath = fs.Path.GetFullPath(fs.Path.Combine(SteamInstallPath, "config"));
        var loginUsersPath = fs.Path.Combine(configPath, "loginusers.vdf");
        fs.File.Delete(loginUsersPath);
    }

    public static void WriteCorruptLoginUsers(this IFileSystem fs)
    {
        var configPath = fs.Path.GetFullPath(fs.Path.Combine(SteamInstallPath, "config"));
        var loginUsersPath = fs.Path.Combine(configPath, "loginusers.vdf");
        fs.File.WriteAllText(loginUsersPath, "\0");
    }

    public static IFileInfo WriteLoginUsers(this IFileSystem fs, params IEnumerable<SteamUserLoginMetadata>? users)
    {
        var configPath = fs.Path.GetFullPath(fs.Path.Combine(SteamInstallPath, "config"));
        var loginUsersPath = fs.Path.Combine(configPath, "loginusers.vdf");

        var content = $@"
""users""
{{
    {SerializeUsers(users)}
}}
";
        fs.File.WriteAllText(loginUsersPath, content);
        return fs.FileInfo.New(loginUsersPath);
    }

    private static string SerializeUsers(IEnumerable<SteamUserLoginMetadata>? users)
    {
        var sb = new StringBuilder();

        if (users is null)
            return string.Empty;

        foreach (var metadata in users)
        {
            var content = $@"
    ""{metadata.UserId}""
    {{
	    ""AccountName""		""someName""
	    ""PersonaName""		""some Name""
	    ""RememberPassword""		""1""
	    ""WantsOfflineMode""		""{BoolToNumber(metadata.UserWantsOffline)}""
	    ""SkipOfflineModeWarning""		""0""
	    ""AllowAutoLogin""		""1""
	    ""MostRecent""		""{BoolToNumber(metadata.MostRecent)}""
	    ""Timestamp""		""0000000000""
    }}";

            sb.AppendLine(content);

        }

        return sb.ToString();
    }


    private static int BoolToNumber(bool value)
    {
        return value ? 1 : 0;
    }
}