using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using Xunit;

namespace AET.SteamAbstraction.Test.TestUtilities;

internal static partial class SteamInstallation
{
    public static void InstallCorruptApp(this ISteamLibrary library, IFileSystem fileSystem)
    {
        var randomAcfName = fileSystem.Path.GetRandomFileName() + ".acf";
        library.SteamAppsLocation.Create();

        fileSystem.File.WriteAllText(fileSystem.Path.Combine(library.SteamAppsLocation.FullName, randomAcfName), "\0");
    }

    public static SteamAppManifest InstallGame(
        this ISteamLibrary library,
        uint id,
        string gameName,
        string appManifestName)
    {
        return InstallGameCore(library, id, appManifestName, gameName);
    }

    public static SteamAppManifest InstallGame(
        this ISteamLibrary library,
        uint id,
        string gameName,
        uint numberDepots = 1,
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        var manifestFileName = $"appmanifest_{id}.acf";
        return InstallGameCore(library, id, manifestFileName, gameName, numberDepots, appState);
    }

    private static SteamAppManifest InstallGameCore(
        this ISteamLibrary library,
        uint id,
        string appManifestName,
        string gameName,
        uint numberDepots = 1,
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        var fs = library.SteamAppsLocation.FileSystem;


        var depotSb = new StringBuilder();
        for (var i = 1; i < numberDepots + 1; i++)
        {
            var depotId = id + i;
            depotSb.Append($@"
    ""{depotId}""
    {{
    }}
");
        }

        var manifestContent = $@"
""AppState""
{{
	""appid""		""{id}""
	""name""		""{gameName}""
	""StateFlags""		""{(int)appState}""
	""installdir""		""{gameName}""
    ""InstalledDepots""
    {{
        {depotSb}
    }}
    ""someProperty""    ""someValue""
}}
";

        var manifestFilePath = fs.Path.Combine(library.SteamAppsLocation.FullName, appManifestName);
        fs.File.WriteAllText(manifestFilePath, manifestContent);

        Assert.True(fs.File.Exists(manifestFilePath));

        var gamePath = fs.Path.Combine(library.CommonLocation.FullName, gameName);
        var gameDir = fs.Directory.CreateDirectory(gamePath);

        return new SteamAppManifest(library, fs.FileInfo.New(manifestFilePath), id, gameName, gameDir,
            SteamAppState.StateFullyInstalled, new HashSet<uint>());
    }
}