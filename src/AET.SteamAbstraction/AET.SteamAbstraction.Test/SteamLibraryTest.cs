using System.Collections;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;

namespace AET.SteamAbstraction.Test;

public class SteamLibraryTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<ISteamAppManifestReader> _reader;

    public SteamLibraryTest()
    {
        var sc = new ServiceCollection();
        _fileSystem = new MockFileSystem();
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _reader = new Mock<ISteamAppManifestReader>();
        sc.AddTransient(_ => _reader.Object);
        _serviceProvider = sc.BuildServiceProvider();
    }

    [Fact]
    public void TestProperties()
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
        Assert.NotNull(lib.CommonLocation);
        Assert.NotNull(lib.SteamAppsLocation);
        Assert.NotNull(lib.WorkshopsLocation);
    }

    [Fact]
    public void TestEquality()
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
        var other = new Mock<ISteamLibrary>();
        other.Setup(o => o.LibraryLocation)
            .Returns(_fileSystem.DirectoryInfo.New("OtherLib"));
        Assert.NotEqual(lib, other.Object);
        other.Setup(o => o.LibraryLocation)
            .Returns(_fileSystem.DirectoryInfo.New("Library"));
        Assert.Equal(lib, other.Object);

        var otherInst = new SteamLibrary(_fileSystem.DirectoryInfo.New("./Library"), _serviceProvider);
        Assert.Equal(lib, otherInst);
        Assert.Equal(lib.GetHashCode(), otherInst.GetHashCode());
    }

    [Fact]
    public void TestNoApps1()
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

        var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
            _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
            new HashSet<uint>());

        _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
            .Returns(app);

        var apps = lib.GetApps();

        Assert.Empty(apps);
    }

    [Fact]
    public void TestNoApps2()
    {
        _fileSystem.Initialize().WithSubdirectory("Library/steamapps");
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

        var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
            _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
            new HashSet<uint>());

        _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
            .Returns(app);

        var apps = lib.GetApps();

        Assert.Empty(apps);
    }

    [Fact]
    public void TestApps()
    {
        _fileSystem.Initialize().WithFile("Library/steamapps/test.acf");
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

        var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
            _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
            new HashSet<uint>());

        _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
            .Returns(app);

        var apps = lib.GetApps();
        var result = Assert.Single((IEnumerable)apps);
        Assert.Same(app, result);
    }

    [Fact]
    public void TestNoDuplicatesApps()
    {
        _fileSystem.Initialize()
            .WithFile("Library/steamapps/test1.acf")
            .WithFile("Library/steamapps/test2.acf");

        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

        var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
            _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
            new HashSet<uint>());

        _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
            .Returns(app);

        var apps = lib.GetApps();
        var result = Assert.Single((IEnumerable)apps);
        Assert.Same(app, result);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void TestLocations_Windows()
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
        Assert.Equal("C:\\Library\\steamapps\\common", lib.CommonLocation.FullName);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void TestLocations_Linux()
    {
        var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
        Assert.Equal("/Library/steamapps/common", lib.CommonLocation.FullName);
    }
}