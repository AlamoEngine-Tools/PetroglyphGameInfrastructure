using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Windows.Test.Steam;

public class SteamVdfReaderTest
{
    private readonly SteamVdfReader _service;
    private readonly MockFileSystem _fileSystem;

    public SteamVdfReaderTest()
    {
        var sc = new ServiceCollection();
        _fileSystem = new MockFileSystem();
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _service = new SteamVdfReader(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestInvalidData_Throws()
    {
        _fileSystem.Initialize().WithFile("input.vdf");
        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<SteamException>(() => _service.ReadLibraryLocationsFromConfig(input));
    }
        
    [Fact]
    public void ReadLibraryLocations()
    {
        var data = @"""libraryfolders""
{
	""contentstatsid""		""-6588270118365286089""
	""0""
	{
		""path""		""C:\\Lib1""
		""label""		""""
	}
	""1""
	{
		""label""		""""
		""path""		""C:\\Lib2""
	}
	""2"" ""C:\\Lib3""
}
";

        var expected1 = _fileSystem.DirectoryInfo.New("C:\\Lib1").FullName;
        var expected2 = _fileSystem.DirectoryInfo.New("C:\\Lib2").FullName;
        var expected3 = _fileSystem.DirectoryInfo.New("C:\\Lib3").FullName;

        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        var locations = _service.ReadLibraryLocationsFromConfig(input).Select(l => l.FullName).ToList();

        Assert.Contains(expected1, locations);
        Assert.Contains(expected2, locations);
        Assert.Contains(expected3, locations);
        Assert.DoesNotContain("-6588270118365286089", locations);
    }

    [Fact]
    public void ReadLibraryLocationsInvalidData_Throws()
    {
        var data = @"""invalidData""
{
	""contentstatsid""		""-6588270118365286089""
}
";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<SteamException>(() => _service.ReadLibraryLocationsFromConfig(input));

    }

    [Fact]
    public void ReadLibraryLocationsNoLibraries()
    {
        var data = @"""libraryfolders""
{
	""contentstatsid""		""-6588270118365286089""
}
";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        var libs = _service.ReadLibraryLocationsFromConfig(input);
        Assert.Empty(libs);
    }

    [Fact]
    public void TestReadAppManifest()
    {
        var data = @"""AppState""
{
	""appid""		""1230""
	""name""		""GameName""
	""StateFlags""		""516""
	""installdir""		""GamePath""
	""InstalledDepots""
	{
		""1231"" { }
		""1232"" { }
		""1233"" { }
	}
}
";
        var lib = new Mock<ISteamLibrary>();
        lib.Setup(l => l.LibraryLocation).Returns(_fileSystem.DirectoryInfo.New("."));
        lib.Setup(l => l.CommonLocation).Returns(_fileSystem.DirectoryInfo.New("steamapps/common"));

        _fileSystem.Initialize().WithFile("steamapps/input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("steamapps/input.vdf");
        var app = _service.ReadManifest(input, lib.Object);

        Assert.Equal(1230u, app.Id);
        Assert.Equal("GameName", app.Name);
        Assert.Equal(SteamAppState.StateFullyInstalled | SteamAppState.StateUpdatePaused, app.State);

        var expectedGameFullPath = _fileSystem.Path.GetFullPath("steamapps/common/GamePath");

        Assert.Equal(expectedGameFullPath, app.InstallDir.FullName);

        Assert.Contains(1231u, app.Depots);
        Assert.Contains(1232u, app.Depots);
        Assert.Contains(1233u, app.Depots);
    }

    [Fact]
    public void TestReadInvalidAppManifest_Throws()
    {
        var data = @"""AppState""
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
";
        var lib = new Mock<ISteamLibrary>();
        lib.Setup(l => l.LibraryLocation).Returns(_fileSystem.DirectoryInfo.New("./"));
        lib.Setup(l => l.CommonLocation).Returns(_fileSystem.DirectoryInfo.New("./steamapps/common"));

        _fileSystem.Initialize().WithFile("steamapps/input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("steamapps/input.vdf");
        Assert.Throws<SteamException>(() => _service.ReadManifest(input, lib.Object));
    }
}