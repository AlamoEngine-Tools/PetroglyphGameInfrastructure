using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AET.Testing.Extensions;
using AnakinRaW.CommonUtilities.FileSystem;
using Xunit;

namespace AET.SteamAbstraction.Testing;

internal sealed partial class TestingSteamInstallationImpl
{
    public ITestingSteamLibrary InstallDefaultLibrary(bool addToConfig = true)
    {
        var steamPath = _fileSystem.Path.GetFullPath(TestingSteamConstants.SteamInstallPath);
        return InstallSteamLibrary(steamPath, true, addToConfig);
    }

    public ITestingSteamLibrary InstallLibrary(string path, bool addToConfig = true)
    {
        return InstallSteamLibrary(path, false, addToConfig);
    }

    private ITestingSteamLibrary InstallSteamLibrary(string path, bool isDefault, bool addToConfig = true)
    {
        var commonPath = _fileSystem.Path.Combine(path, "steamapps", "common");
        var workshopPath = _fileSystem.Path.Combine(path, "steamapps", "workshop");

        _fileSystem.Directory.CreateDirectory(commonPath);
        _fileSystem.Directory.CreateDirectory(workshopPath);

        var libDir = _fileSystem.DirectoryInfo.New(path);
        Assert.True(libDir.Exists);

        var contentId = Random.Long();

        var libraryVdf = $@"""libraryfolder""
{{
	""contentid""		""{contentId}""
	""label""		""""
}}
";

        var libraryVdfName = "libraryfolder.vdf";
        var libraryVdfSubPath = string.Empty;
        if (isDefault)
        {
            libraryVdfName = "libraryfolders.vdf";
            libraryVdfSubPath = "steamapps";
        }

        _fileSystem.File.WriteAllText(_fileSystem.Path.Combine(libDir.FullName, libraryVdfSubPath, libraryVdfName), libraryVdf);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var _ = _fileSystem.File.Create(_fileSystem.Path.Combine(libDir.FullName, "steam.dll"));
        }

        var lib = new TestingSteamLibrary(libDir, _serviceProvider);

        if (addToConfig)
            AddToConfig(lib, _fileSystem);
        return lib;
    }

    private static void AddToConfig(TestingSteamLibrary lib, IFileSystem fs)
    {
        var steamPath = fs.Path.GetFullPath(TestingSteamConstants.SteamInstallPath);
        if (!fs.Directory.Exists(steamPath))
            Assert.Fail("Steam not installed.");

        var configFile = fs.FileInfo.New(fs.Path.Combine(steamPath, "config", "libraryfolders.vdf"));

        var libPaths = new List<string>
        {
            lib.LibraryLocation.FullName
        };

        if (configFile.Exists)
        {
            var currentLibs = SteamVdfReader.ReadLibraryLocationsFromConfig(configFile).ToList();
            foreach (var currentLib in currentLibs)
            {
                if (fs.Path.AreEqual(lib.LibraryLocation.FullName, currentLib.FullName))
                    continue;
                libPaths.Add(currentLib.FullName);
            }
        }

        var fileContent = $@"
""libraryfolders""
{{
    {BuildLibPathsVdf(libPaths)}
}}";

        fs.File.WriteAllText(configFile.FullName, fileContent);
    }

    private static string BuildLibPathsVdf(List<string> libPaths)
    {
        if (libPaths.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        for (int i = 0; i < libPaths.Count; i++)
        {
            var path = libPaths[i];
            var content = $@"
	""{i}""
	{{
		""path""		""{path}""
	}}
";

            sb.AppendLine(content);
        }
        return sb.ToString();
    }
}