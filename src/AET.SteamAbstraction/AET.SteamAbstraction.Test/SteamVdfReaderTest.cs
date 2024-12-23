﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Test.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;
using VdfException = AET.SteamAbstraction.Vdf.VdfException;

namespace AET.SteamAbstraction.Test;

public class SteamVdfReaderTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SteamVdfReader _vdfReader;
    private readonly MockFileSystem _fileSystem = new();

    public SteamVdfReaderTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_ => _fileSystem);
        _serviceProvider = sc.BuildServiceProvider();
        _vdfReader = new SteamVdfReader(_serviceProvider);
    }

    #region ReadLibraryLocationsFromConfig

    [Fact]
    public void ReadLibraryLocationsFromConfig_EmptyFile_Throws()
    {
        _fileSystem.Initialize().WithFile("input.vdf");
        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<VdfException>(() => _vdfReader.ReadLibraryLocationsFromConfig(input).ToList());
    }

    [Fact]
    public void ReadLibraryLocationsFromConfig_InvalidRootNode_Throws()
    {
        var data = @"""invalidData""
{
    ""0""		""/Lib""
}
";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<VdfException>(() => _vdfReader.ReadLibraryLocationsFromConfig(input).ToList());
    }

    [Fact]
    public void ReadLibraryLocationsFromConfig()
    {
        var data = @"""libraryfolders""
{
    ""NaN""		""/LibA""
    ""0""
    {
        ""path""		""/Lib1""
        ""label""		""""
    }
    ""1""
    {
        ""label""		""""
        ""path""		""/Lib2""
    }
    ""2"" ""/Lib3""
    ""99""
    {
        ""label""		""LabelA""
    }
}
";

        var expected1 = _fileSystem.DirectoryInfo.New("/Lib1").FullName;
        var expected2 = _fileSystem.DirectoryInfo.New("/Lib2").FullName;
        var expected3 = _fileSystem.DirectoryInfo.New("/Lib3").FullName;

        var notExpected1 = _fileSystem.DirectoryInfo.New("/LibA").FullName;
        var notExpected2 = _fileSystem.DirectoryInfo.New("/LabelA").FullName;

        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        var locations = _vdfReader.ReadLibraryLocationsFromConfig(input).Select(l => l.FullName).ToList();

        Assert.Contains(expected1, locations);
        Assert.Contains(expected2, locations);
        Assert.Contains(expected3, locations);
        Assert.DoesNotContain(notExpected1, locations);
        Assert.DoesNotContain(notExpected2, locations);
    }

    [Fact]
    public void ReadLibraryLocationsFromConfig_NoLibrariesFound_EmptyEnumerable()
    {
        var data = @"""libraryfolders""
{
    ""NaN""		""/Lib""
}
";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        var libs = _vdfReader.ReadLibraryLocationsFromConfig(input);
        Assert.Empty(libs);
    }

    [Fact]
    public void ReadLibraryLocationsFromConfig_EmptyContent_EmptyEnumerable()
    {
        var data = @"""libraryfolders""
{
}
";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        var libs = _vdfReader.ReadLibraryLocationsFromConfig(input);
        Assert.Empty(libs);
    }

    #endregion

    #region ReadManifest

    [Fact]
    public void ReadManifest_NullArgs_Throws()
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider);
        Assert.Throws<ArgumentNullException>(() => _vdfReader.ReadManifest(null!, lib));
        Assert.Throws<ArgumentNullException>(() => _vdfReader.ReadManifest(_fileSystem.FileInfo.New("path"), null!));
    }

    [Fact]
    public void ReadManifest_CorrectManifest()
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider);

        var expectedManifest = lib.InstallGame(
            1230,
            "MyGame",
            3,
            SteamAppState.StateFullyInstalled | SteamAppState.StateUpdatePaused);

        var app = _vdfReader.ReadManifest(expectedManifest.ManifestFile, lib);

        Assert.Equal(1230u, app.Id);
        Assert.Equal("MyGame", app.Name);
        Assert.Equal(SteamAppState.StateFullyInstalled | SteamAppState.StateUpdatePaused, app.State);
        Assert.Equal(expectedManifest.InstallDir.FullName, app.InstallDir.FullName);
        Assert.Contains(1231u, app.Depots);
        Assert.Contains(1232u, app.Depots);
        Assert.Contains(1233u, app.Depots);
    }

    public static IEnumerable<object[]> GetInvalidAppManifestContent()
    {
        yield return ["\0"];

        // Invalid Root Node
        yield return [@"
""SomeNode""
{
    ""appid""		""1230""
    ""name""		""MyGame""
    ""StateFlags""		""516""
    ""installdir""		""GamePath""
    ""InstalledDepots""
    {
        ""1231"" { }
        ""1232"" { }
        ""1233"" { }
    }
}
"];


        // Missing Name
        yield return [@"
""AppState""
{
    ""appid""		""1230""
    ""StateFlags""		""516""
    ""installdir""		""GamePath""
    ""InstalledDepots""
    {
        ""1231"" { }
        ""1232"" { }
        ""1233"" { }
    }
}
"];

        // Invalid ID
        yield return [@"
""AppState""
{
    ""appid""		""-1230""
    ""name""		""MyGame""
    ""StateFlags""		""516""
    ""installdir""		""GamePath""
    ""InstalledDepots""
    {
        ""1231"" { }
        ""1232"" { }
        ""1233"" { }
    }
}
"];

        // Missing ID
        yield return [@"
""AppState""
{
    ""name""		""1230""
    ""StateFlags""		""516""
    ""installdir""		""GamePath""
    ""InstalledDepots""
    {
        ""1231"" { }
        ""1232"" { }
        ""1233"" { }
    }
}
"];

        // Empty Name
        yield return [@"
""AppState""
{
    ""appid""		""1230""
    ""name""		""""
    ""StateFlags""		""516""
    ""installdir""		""GamePath""
    ""InstalledDepots""
    {
        ""1231"" { }
        ""1232"" { }
        ""1233"" { }
    }
}
"];

        // Missing State
        yield return [@"
""AppState""
{
    ""appid""		""1230""
    ""name""		""MyGame""
    ""installdir""		""GamePath""
    ""InstalledDepots""
    {
        ""1231"" { }
        ""1232"" { }
        ""1233"" { }
    }
}
"];

        // Missing Depots
        yield return [@"
""AppState""
{
    ""appid""		""1230""
    ""name""		""MyGame""
    ""StateFlags""		""516""
    ""installdir""		""GamePath""
}
"];

        // Invalid Depot Property Kind
        yield return [@"
""AppState""
{
    ""appid""		""1230""
    ""name""		""MyGame""
    ""installdir""		""GamePath""
    ""InstalledDepots""  ""This is not a Object""
}
"];
    }

    [Theory]
    [MemberData(nameof(GetInvalidAppManifestContent))]
    public void ReadManifest_InvalidAppManifest_Throws(string content)
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider);

        var manifestFile = _fileSystem.Path.Combine(lib.SteamAppsLocation.FullName, "manifest.acf");
        _fileSystem.File.WriteAllText(manifestFile, content);

        Assert.ThrowsAny<SteamException>(() => _vdfReader.ReadManifest(_fileSystem.FileInfo.New(manifestFile), lib));
    }

    [Fact]
    public void ReadManifest_ManifestNotPartOfLibrary_Throws()
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider);
        var otherLib = _fileSystem.InstallSteamLibrary("otherLib", _serviceProvider);

        var manifestFile = otherLib.InstallGame(1230, "MyGame").ManifestFile;

        Assert.Throws<SteamException>(() => _vdfReader.ReadManifest(manifestFile, lib));
    }

    #endregion
}