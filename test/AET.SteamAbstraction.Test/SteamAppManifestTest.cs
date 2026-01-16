using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Testing.TestBases;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamAppManifestTest : SteamTestBase
{
    private readonly MockFileSystem _fileSystem = new();

    public SteamAppManifestTest()
    {
        Steam.InstallSteamFilesOnly();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IFileSystem>(_fileSystem);
        serviceCollection.AddSingleton<IRegistry>(new InMemoryRegistry());
    }

    [Fact]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SteamAppManifest(null!, _fileSystem.FileInfo.New("file.acf"), 0, "name", _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint>()));

        var lib = Steam.InstallLibrary("path", false);
        Assert.Throws<ArgumentNullException>(() => new SteamAppManifest(lib, null!, 0, "name", _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint>()));

        Assert.Throws<ArgumentNullException>(() => new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), 0, null!, _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint>()));

        Assert.Throws<ArgumentNullException>(() => new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), 0, "name", null!,
            SteamAppState.StateFullyInstalled, new HashSet<uint>()));

        Assert.Throws<ArgumentNullException>(() => new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), 0, "name", _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, null!));

        Assert.Throws<ArgumentException>(() => new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), 0, string.Empty, _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint>()));
    }

    [Fact]
    public void Ctor_SetsProperties()
    {
        var lib = Steam.InstallLibrary("path", false);
        var appManifest = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), 123, "name",
            _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint> {987, 654});

        Assert.Same(appManifest.Library, lib);
        Assert.Equal(appManifest.ManifestFile.FullName, _fileSystem.FileInfo.New("file.acf").FullName);
        Assert.Equal(123u, appManifest.Id);
        Assert.Equal("name", appManifest.Name);
        Assert.Equal(appManifest.InstallDir.FullName, _fileSystem.DirectoryInfo.New("path").FullName);
        Assert.Equal(SteamAppState.StateFullyInstalled, appManifest.State);
        Assert.Equivalent(new HashSet<uint> { 654, 987 }, appManifest.Depots, true);
    }

    [Fact]
    public void Equality()
    {
        var lib = Steam.InstallLibrary("path", false);
        var otherLib = Steam.InstallLibrary("other", false);

        const uint aId = 123;
        const uint bId = 456;
        
        var appA = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), aId, "name",
            _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint> { 987, 654 });

        var appB = new SteamAppManifest(lib, _fileSystem.FileInfo.New(_fileSystem.Path.GetRandomFileName()), aId, _fileSystem.Path.GetRandomFileName(),
            _fileSystem.DirectoryInfo.New(_fileSystem.Path.GetRandomFileName()),
            Random.Enum<SteamAppState>(), new HashSet<uint>());

        var appOtherLib = new SteamAppManifest(otherLib, _fileSystem.FileInfo.New("file.acf"), aId, "name",
            _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint> { 987, 654 });

        var appOtherId = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file.acf"), bId, "name",
            _fileSystem.DirectoryInfo.New("path"),
            SteamAppState.StateFullyInstalled, new HashSet<uint> { 987, 654 });

        Assert.True(appA.Equals((object)appA));
        Assert.True(appA.Equals(appA));
        Assert.Equal(appA.GetHashCode(), appA.GetHashCode());

        Assert.True(appA.Equals((object)appB));
        Assert.True(appA.Equals(appB));
        Assert.Equal(appA, appB);
        Assert.Equal(appA.GetHashCode(), appB.GetHashCode());

        Assert.False(appA.Equals((object)appOtherLib));
        Assert.False(appA.Equals(appOtherLib));
        Assert.NotEqual(appA, appOtherLib);
        Assert.NotEqual(appA.GetHashCode(), appOtherLib.GetHashCode());

        Assert.False(appA.Equals((object)appOtherId));
        Assert.False(appA.Equals(appOtherId));
        Assert.NotEqual(appA.GetHashCode(), appOtherId.GetHashCode());
    }
}