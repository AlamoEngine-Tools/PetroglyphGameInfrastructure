using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Testing;
using AET.Testing.Attributes;
using AnakinRaW.CommonUtilities.Registry;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamLibraryFinderTest : IDisposable
{
    private readonly SteamLibraryFinder _libraryFinder;
    private readonly MockFileSystem _fileSystem = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ITestingSteamInstallation _steam;

    public SteamLibraryFinderTest()
    {
        var sc = new ServiceCollection();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            sc.AddSingleton<IRegistry>(new InMemoryRegistry(InMemoryRegistryCreationFlags.Default));
        sc.AddSingleton<IFileSystem>(_fileSystem);
        SteamAbstractionLayer.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _libraryFinder = new SteamLibraryFinder(_serviceProvider);
        
        _steam = _fileSystem.Steam(_serviceProvider);
        _steam.Install();
    }

    public void Dispose()
    {
        _steam.Dispose();
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_SteamNotFound_ReturnsEmpty()
    {
        Assert.Empty(_libraryFinder.FindLibraries(_fileSystem.DirectoryInfo.New("not steam")));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_NoLibrariesInstalled()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.Steam(_serviceProvider).Install();
        var libs = _libraryFinder.FindLibraries(registry.InstallationDirectory!);
        Assert.Empty(libs);
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryInstalled()
    {
        using var registry = _serviceProvider.GetRequiredService<ISteamRegistryFactory>().CreateRegistry();
        _fileSystem.Steam(_serviceProvider).Install();
        var lib = _steam.InstallDefaultLibrary(addToConfig: true);

        var libs = _libraryFinder.FindLibraries(registry.InstallationDirectory!);

        Assert.Equal([lib], libs);
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryInstalledButNotInConfig_NotFound()
    {
        _steam.InstallDefaultLibrary(addToConfig: false);
        var libs = _libraryFinder.FindLibraries(_steam.InstallationDirectory!);
        Assert.Empty(libs);
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled()
    {
        var defaultLib = _steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = _steam.InstallLibrary("externalLib");

        var libs = _libraryFinder.FindLibraries(_steam.InstallationDirectory!);

        Assert.Equal(
            new List<ISteamLibrary> { defaultLib, externalLib }.OrderBy(x => x.LibraryLocation.FullName),
            libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_Windows_DefaultLibraryAndExternalLibInstalled_ExternalDoesNotHaveSteamDll_Skip()
    {
        var defaultLib = _steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = _steam.InstallLibrary("externalLib");
        
        _fileSystem.File.Delete(_fileSystem.Path.Combine(externalLib.LibraryLocation.FullName, "steam.dll"));

        var libs = _libraryFinder.FindLibraries(_steam.InstallationDirectory!);

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled_ExternalDoesNotHaveVdf_Skip()
    {
        var defaultLib = _steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = _steam.InstallLibrary("externalLib");

        _fileSystem.File.Delete(_fileSystem.Path.Combine(externalLib.LibraryLocation.FullName, "libraryfolder.vdf"));

        var libs = _libraryFinder.FindLibraries(_steam.InstallationDirectory!);

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled_ExternalFolderDoesNotExist()
    {
        var defaultLib = _steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = _steam.InstallLibrary("externalLib");

        _fileSystem.Directory.Delete(externalLib.LibraryLocation.FullName, true);

        var libs = _libraryFinder.FindLibraries(_steam.InstallationDirectory!);

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }
}