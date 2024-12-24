using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Test.TestUtilities;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamLibraryFinderTest
{
    private readonly SteamLibraryFinder _libraryFinder;
    private readonly MockFileSystem _fileSystem = new();
    private readonly IServiceProvider _serviceProvider;

    public SteamLibraryFinderTest()
    {
        var sc = new ServiceCollection();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            sc.AddSingleton<IRegistry>(new InMemoryRegistry(InMemoryRegistryCreationFlags.Default));
        sc.AddSingleton<IFileSystem>(_fileSystem);
        SteamAbstractionLayer.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _libraryFinder = new SteamLibraryFinder(_serviceProvider);
    }

    [Fact]
    public void FindLibraries_SteamNotFound_Throws()
    {
        Assert.Throws<SteamException>(_libraryFinder.FindLibraries);
    }

    [Fact]
    public void FindLibraries_NoLibrariesInstalled()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        var libs = _libraryFinder.FindLibraries();
        Assert.Empty(libs);
    }

    [Fact]
    public void FindLibraries_DefaultLibraryInstalled()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        var lib = _fileSystem.InstallDefaultLibrary(_serviceProvider, addToConfig: true);

        var libs = _libraryFinder.FindLibraries();

        Assert.Equal([lib], libs);
    }

    [Fact]
    public void FindLibraries_DefaultLibraryInstalledButNotInConfig_NotFound()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        _fileSystem.InstallDefaultLibrary(_serviceProvider, addToConfig: false);

        var libs = _libraryFinder.FindLibraries();

        Assert.Empty(libs);
    }

    [Fact]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        var defaultLib = _fileSystem.InstallDefaultLibrary(_serviceProvider, addToConfig: true);
        var externalLib = _fileSystem.InstallSteamLibrary("externalLib", _serviceProvider);

        var libs = _libraryFinder.FindLibraries();

        Assert.Equal(
            new List<ISteamLibrary> { defaultLib, externalLib }.OrderBy(x => x.LibraryLocation.FullName),
            libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_Windows_DefaultLibraryAndExternalLibInstalled_ExternalDoesNotHaveSteamDll_Skip()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        var defaultLib = _fileSystem.InstallDefaultLibrary(_serviceProvider, addToConfig: true);
        var externalLib = _fileSystem.InstallSteamLibrary("externalLib", _serviceProvider);
        
        _fileSystem.File.Delete(_fileSystem.Path.Combine(externalLib.LibraryLocation.FullName, "steam.dll"));

        var libs = _libraryFinder.FindLibraries();

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    [Fact]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled_ExternalDoesNotHaveVdf_Skip()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        var defaultLib = _fileSystem.InstallDefaultLibrary(_serviceProvider, addToConfig: true);
        var externalLib = _fileSystem.InstallSteamLibrary("externalLib", _serviceProvider);

        _fileSystem.File.Delete(_fileSystem.Path.Combine(externalLib.LibraryLocation.FullName, "libraryfolder.vdf"));

        var libs = _libraryFinder.FindLibraries();

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    [Fact]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled_ExternalFolderDoesNotExist()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.InstallSteam(registry);
        var defaultLib = _fileSystem.InstallDefaultLibrary(_serviceProvider, addToConfig: true);
        var externalLib = _fileSystem.InstallSteamLibrary("externalLib", _serviceProvider);

        _fileSystem.Directory.Delete(externalLib.LibraryLocation.FullName, true);

        var libs = _libraryFinder.FindLibraries();

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }
}