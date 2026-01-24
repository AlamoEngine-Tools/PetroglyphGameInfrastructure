// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using Xunit;

namespace AET.SteamAbstraction.Testing;

internal sealed class TestingSteamLibrary(IDirectoryInfo libraryLocation, IServiceProvider serviceProvider)
    : SteamLibrary(libraryLocation, serviceProvider), ITestingSteamLibrary
{
    public SteamAppManifest InstallGame(uint id, string gameName, string appManifestName)
    {
        return InstallGameCore(id, appManifestName, gameName, [id + 1]);
    }

    public SteamAppManifest InstallGame(
        uint id, 
        string gameName,
        uint numberDepots = 1,
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        var manifestFileName = $"appmanifest_{id}.acf";
        var depots = new List<uint>();
        for (uint i = 1; i < numberDepots + 1; i++)
            depots.Add(i);
        return InstallGameCore(id, manifestFileName, gameName, depots, appState);
    }

    public SteamAppManifest InstallGame(
        uint id, 
        string gameName, 
        IList<uint> depots,
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        var manifestFileName = $"appmanifest_{id}.acf";
        return InstallGameCore(id, manifestFileName, gameName, depots, appState);
    }

    public void InstallCorruptApp()
    {
        var randomAcfName = FileSystem.Path.GetRandomFileName() + ".acf";
        SteamAppsLocation.Create();

        FileSystem.File.WriteAllText(FileSystem.Path.Combine(SteamAppsLocation.FullName, randomAcfName), "\0");
    }

    private SteamAppManifest InstallGameCore(
        uint id,
        string appManifestName,
        string gameName,
        IList<uint> depots,
        SteamAppState appState = SteamAppState.StateFullyInstalled)
    {
        var depotSb = new StringBuilder();
        foreach (var depot in depots)
        {
            depotSb.Append($@"
    ""{depot}""
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

        var manifestFilePath = FileSystem.Path.Combine(SteamAppsLocation.FullName, appManifestName);
        FileSystem.File.WriteAllText(manifestFilePath, manifestContent);

        Assert.True(FileSystem.File.Exists(manifestFilePath));

        var gamePath = FileSystem.Path.Combine(CommonLocation.FullName, gameName);
        var gameDir = FileSystem.Directory.CreateDirectory(gamePath);

        return new SteamAppManifest(
            this,
            FileSystem.FileInfo.New(manifestFilePath), 
            id, 
            gameName,
            gameDir,
            SteamAppState.StateFullyInstalled, 
            new HashSet<uint>());
    }
}