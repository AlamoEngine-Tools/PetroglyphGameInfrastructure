using System;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Test.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamLibraryTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MockFileSystem _fileSystem = new();

    public SteamLibraryTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_ => _fileSystem);
        SteamAbstractionLayer.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();

        _fileSystem.InstallSteamFiles();
    }

    [Fact]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SteamLibrary(null!, _serviceProvider));
        Assert.Throws<ArgumentNullException>(() => new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), null!));
    }

    [Theory]
    [InlineData("Library", true, true)]
    [InlineData("./Library", true, true)]
    [InlineData("library", true, false)]
    [InlineData("other", false, false)]
    public void TestEquality(string path, bool equalWindows, bool equalLinux)
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
        var other = new SteamLibrary(_fileSystem.DirectoryInfo.New(path), _serviceProvider);

        var equal = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? equalWindows : equalLinux;

        Assert.Equal(equal, lib.Equals(other));
        Assert.Equal(equal, lib.GetHashCode().Equals(other.GetHashCode()));
    }

    [Fact]
    public void TestEquality_NullAndSame()
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

        Assert.False(lib.Equals((object)null!));
        Assert.False(lib.Equals(null));

        Assert.True(lib.Equals(lib));
        Assert.True(lib.Equals((object)lib));
    }

    [Fact]
    public void Locations()
    {
        var libDirBasePath = _fileSystem.DirectoryInfo.New("Library");
        var expectedSteamApps = _fileSystem.Path.Combine(libDirBasePath.FullName, "steamapps");
        var expectedCommon = _fileSystem.Path.Combine(expectedSteamApps, "common");
        var expectedWorkshop = _fileSystem.Path.Combine(expectedSteamApps, "workshop");

        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
        Assert.Equal(expectedSteamApps, lib.SteamAppsLocation.FullName);
        Assert.Equal(expectedCommon, lib.CommonLocation.FullName);
        Assert.Equal(expectedWorkshop, lib.WorkshopsLocation.FullName);
    }

    [Fact]
    public void GetApps_NoAppsInstalled_Empty()
    {
        var library = _fileSystem.InstallSteamLibrary("Library", _serviceProvider);
        
        var apps = library.GetApps();
        
        Assert.Empty(apps);
    }

    [Fact]
    public void GetApps_NoSteamAppsDirectoryDoesNotExists_Empty()
    {
        var library = _fileSystem.InstallSteamLibrary("Library", _serviceProvider);
        library.SteamAppsLocation.Delete(true);

        var apps = library.GetApps();
        
        Assert.Empty(apps);
    }

    [Fact]
    public void GetApps_InvalidAcfFile_Skipped()
    {
        var library = _fileSystem.InstallSteamLibrary("Library", _serviceProvider);
        library.InstallCorruptApp(_fileSystem);
        
        var apps = library.GetApps();
        
        Assert.Empty(apps);
    }

    [Fact]
    public void TestApps()
    {
        var library = _fileSystem.InstallSteamLibrary("Library", _serviceProvider);
        
        library.InstallCorruptApp(_fileSystem);
        library.InstallGame(123, "Game1", "manifest1.acf");
        var expectedGame2 = library.InstallGame(456, "Game2");
        library.InstallGame(123, "NotGame1", "manifest2.acf");
        library.InstallGame(789, "Ignore", "ignore.vdf");

        var apps = library.GetApps().ToList();

        // Corrupt app should be ignored, duplicate id should be ignored, only *.acf files are used
        Assert.Equal(2, apps.Count);
        Assert.Contains(apps, x => x.Equals(expectedGame2));

        // Cannot assert on game's name, because we cannot know which game will be picked first by file enumeration.
        Assert.Contains(apps, x => x.Id == 123 && x.Library.Equals(library));
    }
}