using System.Collections.Generic;
using System.Linq;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Testing.TestBases;
using AnakinRaW.CommonUtilities.Testing.Attributes;
using Xunit;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace AET.SteamAbstraction.Test;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class SteamLibraryFinderTest : InMemorySteamTestBase
{
    private readonly SteamLibraryFinder _libraryFinder;

    public SteamLibraryFinderTest()
    {
        _libraryFinder = new SteamLibraryFinder(ServiceProvider);
        Steam.Install();
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_SteamNotFound_ReturnsEmpty()
    {
        Assert.Empty(_libraryFinder.FindLibraries(FileSystem.DirectoryInfo.New("not steam")));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_NoLibrariesInstalled()
    {
        var libs = _libraryFinder.FindLibraries(Steam.Registry.InstallationDirectory!);
        Assert.Empty(libs);
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryInstalled()
    {
        var lib = Steam.InstallDefaultLibrary(addToConfig: true);
        var libs = _libraryFinder.FindLibraries(Steam.Registry.InstallationDirectory!);

        Assert.Equal([lib], libs);
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryInstalledButNotInConfig_NotFound()
    {
        Steam.InstallDefaultLibrary(addToConfig: false);
        var libs = _libraryFinder.FindLibraries(Steam.InstallationDirectory!);
        Assert.Empty(libs);
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled()
    {
        var defaultLib = Steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = Steam.InstallLibrary("externalLib");

        var libs = _libraryFinder.FindLibraries(Steam.InstallationDirectory!);

        Assert.Equal(
            new List<ISteamLibrary> { defaultLib, externalLib }.OrderBy(x => x.LibraryLocation.FullName),
            libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_Windows_DefaultLibraryAndExternalLibInstalled_ExternalDoesNotHaveSteamDll_Skip()
    {
        var defaultLib = Steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = Steam.InstallLibrary("externalLib");

        FileSystem.File.Delete(FileSystem.Path.Combine(externalLib.LibraryLocation.FullName, "steam.dll"));

        var libs = _libraryFinder.FindLibraries(Steam.InstallationDirectory!);

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled_ExternalDoesNotHaveVdf_Skip()
    {
        var defaultLib = Steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = Steam.InstallLibrary("externalLib");

        FileSystem.File.Delete(FileSystem.Path.Combine(externalLib.LibraryLocation.FullName, "libraryfolder.vdf"));

        var libs = _libraryFinder.FindLibraries(Steam.InstallationDirectory!);

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }

    // TODO: Target all platforms
    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void FindLibraries_DefaultLibraryAndExternalLibInstalled_ExternalFolderDoesNotExist()
    {
        var defaultLib = Steam.InstallDefaultLibrary(addToConfig: true);
        var externalLib = Steam.InstallLibrary("externalLib");

        FileSystem.Directory.Delete(externalLib.LibraryLocation.FullName, true);

        var libs = _libraryFinder.FindLibraries(Steam.InstallationDirectory!);

        Assert.Equal([defaultLib], libs.OrderBy(x => x.LibraryLocation.FullName));
    }
}