using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Testing.Installation;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;
using VdfException = AET.SteamAbstraction.Vdf.VdfException;

namespace AET.SteamAbstraction.Test;

public class SteamVdfReaderTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MockFileSystem _fileSystem = new();

    public SteamVdfReaderTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_ => _fileSystem);
        _serviceProvider = sc.BuildServiceProvider();
    }

    #region ReadLibraryLocationsFromConfig

    [Fact]
    public void ReadLibraryLocationsFromConfig_EmptyFile_Throws()
    {
        _fileSystem.Initialize().WithFile("input.vdf");
        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<VdfException>(() => SteamVdfReader.ReadLibraryLocationsFromConfig(input).ToList());
    }

    [Fact]
    public void ReadLibraryLocationsFromConfig_InvalidRootNodeName_Throws()
    {
        var data = @"""invalidData""
{
    ""0""		""/Lib""
}
";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<VdfException>(() => SteamVdfReader.ReadLibraryLocationsFromConfig(input).ToList());
    }

    [Fact]
    public void ReadLibraryLocationsFromConfig_InvalidRootNodeKind_Throws()
    {
        var data = @"""libraryfolders"" ""someData""";
        _fileSystem.Initialize().WithFile("input.vdf").Which(d => d.HasStringContent(data));

        var input = _fileSystem.FileInfo.New("input.vdf");
        Assert.Throws<VdfException>(() => SteamVdfReader.ReadLibraryLocationsFromConfig(input).ToList());
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
        var locations = SteamVdfReader.ReadLibraryLocationsFromConfig(input).Select(l => l.FullName).ToList();

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
        var libs = SteamVdfReader.ReadLibraryLocationsFromConfig(input);
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
        var libs = SteamVdfReader.ReadLibraryLocationsFromConfig(input);
        Assert.Empty(libs);
    }

    #endregion

    #region ReadManifest

    [Fact]
    public void ReadManifest_NullArgs_Throws()
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider, false);
        Assert.Throws<ArgumentNullException>(() => SteamVdfReader.ReadManifest(null!, lib));
        Assert.Throws<ArgumentNullException>(() => SteamVdfReader.ReadManifest(_fileSystem.FileInfo.New("path"), null!));
    }

    [Fact]
    public void ReadManifest_CorrectManifest()
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider, false);

        var expectedManifest = lib.InstallGame(
            1230,
            "MyGame",
            [1231, 1232, 1233],
            SteamAppState.StateFullyInstalled | SteamAppState.StateUpdatePaused);

        var app = SteamVdfReader.ReadManifest(expectedManifest.ManifestFile, lib);

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
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider, false);

        var manifestFile = _fileSystem.Path.Combine(lib.SteamAppsLocation.FullName, "manifest.acf");
        _fileSystem.File.WriteAllText(manifestFile, content);

        Assert.ThrowsAny<SteamException>(() => SteamVdfReader.ReadManifest(_fileSystem.FileInfo.New(manifestFile), lib));
    }

    [Fact]
    public void ReadManifest_ManifestNotPartOfLibrary_Throws()
    {
        var lib = _fileSystem.InstallSteamLibrary("steamLib", _serviceProvider, false);
        var otherLib = _fileSystem.InstallSteamLibrary("otherLib", _serviceProvider, false);

        var manifestFile = otherLib.InstallGame(1230, "MyGame").ManifestFile;

        Assert.Throws<SteamException>(() => SteamVdfReader.ReadManifest(manifestFile, lib));
    }

    #endregion

    #region ReadLoginUsers

    [Fact]
    public void ReadLoginUsers_NullFile_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SteamVdfReader.ReadLoginUsers(null!));
    }

    [Fact]
    public void ReadLoginUsers_FileNotExists_Throws()
    {
        var e = Assert.ThrowsAny<SteamException>(() => SteamVdfReader.ReadLoginUsers(_fileSystem.FileInfo.New("NotExists.vdf")));
        Assert.IsType<FileNotFoundException>(e.InnerException);
    }

    public static IEnumerable<object[]> GetInvalidLoginUserContent()
    {
        yield return [string.Empty];
        yield return ["\0"];
        yield return [@"""users"" ""value"""];
        yield return [@"
""notUsers""
{
	""123456789""
	{
		""AccountName""		""some_user""
		""PersonaName""		""someUser""
		""RememberPassword""		""1""
		""WantsOfflineMode""		""0""
		""SkipOfflineModeWarning""		""0""
		""AllowAutoLogin""		""1""
		""MostRecent""		""1""
		""Timestamp""		""000000000""
	}
}"];
        yield return [@"
""users""
{
	""123456789""
	{
		""AccountName""		""some_user""
		""PersonaName""		""someUser""
		""RememberPassword""		""1""
		""WantsOfflineMode""		""notABoolean""
		""SkipOfflineModeWarning""		""0""
		""AllowAutoLogin""		""1""
		""MostRecent""		""1""
		""Timestamp""		""000000000""
	}
}"];
        yield return [@"
""users""
{
	""123456789""
	{
		""AccountName""		""some_user""
		""PersonaName""		""someUser""
		""RememberPassword""		""1""
		""WantsOfflineMode""		""0""
		""SkipOfflineModeWarning""		""0""
		""AllowAutoLogin""		""1""
		""MostRecent""		""-9999""
		""Timestamp""		""000000000""
	}
}"];
        yield return [@"
""users""
{
	""123456789"" ""value""
}"];
    }

    [Theory]
    [MemberData(nameof(GetInvalidLoginUserContent))]
    public void ReadLoginUsers_FileWithInvalidContent(string content)
    {
        _fileSystem.File.WriteAllText("loginusers.vdf", content);
        var file = _fileSystem.FileInfo.New("loginusers.vdf");
        
        Assert.ThrowsAny<SteamException>(() => SteamVdfReader.ReadLoginUsers(file));
    }

    [Fact]
    public void ReadLoginUsers_FileWithNotParsableUserIdIsSkipped()
    {
        var content = @"
""users""
{
	""18446744073709551615""
	{
		""AccountName""		""some_user""
		""PersonaName""		""some user""
		""RememberPassword""		""1""
		""WantsOfflineMode""		""0""
		""SkipOfflineModeWarning""		""0""
		""AllowAutoLogin""		""1""
		""MostRecent""		""1""
		""Timestamp""		""00000000""
	}
    ""InvalidUser""
    {
	    ""AccountName""		""InvalidUser""
	    ""PersonaName""		""Invalid User""
	    ""RememberPassword""		""1""
	    ""WantsOfflineMode""		""0""
	    ""SkipOfflineModeWarning""		""0""
	    ""AllowAutoLogin""		""1""
	    ""MostRecent""		""1""
	    ""Timestamp""		""00000000""
    }
}";
        _fileSystem.File.WriteAllText("loginusers.vdf", content);
        var file = _fileSystem.FileInfo.New("loginusers.vdf");

        var user = Assert.Single(SteamVdfReader.ReadLoginUsers(file).Users);
        Assert.Equal(18446744073709551615u, user.UserId);
        Assert.False(user.UserWantsOffline);
        Assert.True(user.MostRecent);
    }

    [Fact]
    public void ReadLoginUsers_UseDefaults()
    {
        var content = @"
""users""
{
	""123456789""
	{
	}
}
";
        _fileSystem.File.WriteAllText("loginusers.vdf", content);
        var file = _fileSystem.FileInfo.New("loginusers.vdf");

        var user = Assert.Single(SteamVdfReader.ReadLoginUsers(file).Users);
        Assert.Equal(123456789u, user.UserId);
        Assert.False(user.UserWantsOffline);
        Assert.False(user.MostRecent);
    }

    [Fact]
    public void ReadLoginUsers_EmptyFile()
    {
        var content = @"
""users""
{
}
";
        _fileSystem.File.WriteAllText("loginusers.vdf", content);
        var file = _fileSystem.FileInfo.New("loginusers.vdf");

        Assert.Empty(SteamVdfReader.ReadLoginUsers(file).Users);
    }

    [Fact]
    public void ReadLoginUsers()
    {
        _fileSystem.InstallSteamFiles();

        var expectedUsers = new List<SteamUserLoginMetadata>
        {
            new(1, true, false),
            new(2, false, false),
            new(3, false, true),
            new(4, true, true),
        };

        var file = _fileSystem.WriteLoginUsers(expectedUsers);

        var users = SteamVdfReader.ReadLoginUsers(file);

        Assert.Equal(expectedUsers, users.Users.OrderBy(x => x.UserId).ToList(), new SteamUserLoginMetadataEqualityComparer());
    }

    private class SteamUserLoginMetadataEqualityComparer : IEqualityComparer<SteamUserLoginMetadata>
    {
        public bool Equals(SteamUserLoginMetadata x, SteamUserLoginMetadata y)
        {
            return x.UserId == y.UserId && x.MostRecent == y.MostRecent && x.UserWantsOffline == y.UserWantsOffline;
        }

        public int GetHashCode(SteamUserLoginMetadata obj)
        {
            return HashCode.Combine(obj.UserId, obj.MostRecent, obj.UserWantsOffline);
        }
    }

    #endregion
}