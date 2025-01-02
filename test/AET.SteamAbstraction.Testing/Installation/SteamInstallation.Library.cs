using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AET.SteamAbstraction.Library;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.TestingUtilities;
using Xunit;

namespace AET.SteamAbstraction.Testing.Installation;

internal static partial class SteamInstallation
{
    public static ISteamLibrary InstallDefaultLibrary(this IFileSystem fs, IServiceProvider serviceProvider, bool addToConfig = true)
    {
        var steamPath = fs.Path.GetFullPath(SteamInstallPath);
        return InstallSteamLibrary(fs, steamPath, true, serviceProvider, addToConfig);

    }

    public static ISteamLibrary InstallSteamLibrary(
        this IFileSystem fs, 
        string path, 
        IServiceProvider serviceProvider, 
        bool addToConfig = true)
    {
        return InstallSteamLibrary(fs, path, false, serviceProvider, addToConfig);
    }

    private static ISteamLibrary InstallSteamLibrary(
        this IFileSystem fs,
        string path,
        bool isDefault,
        IServiceProvider serviceProvider,
        bool addToConfig = true)
    {
        var commonPath = fs.Path.Combine(path, "steamapps", "common");
        var workshopPath = fs.Path.Combine(path, "steamapps", "workshop");

        fs.Directory.CreateDirectory(commonPath);
        fs.Directory.CreateDirectory(workshopPath);

        var libDir = fs.DirectoryInfo.New(path);
        Assert.True(libDir.Exists);

        var contentId = TestHelpers.RandomLong();

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

        fs.File.WriteAllText(fs.Path.Combine(libDir.FullName, libraryVdfSubPath, libraryVdfName), libraryVdf);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var _ = fs.File.Create(fs.Path.Combine(libDir.FullName, "steam.dll"));
        }

        var lib = new SteamLibrary(libDir, serviceProvider);

        if (addToConfig)
            AddToConfig(lib, fs);
        return lib;
    }

    private static void AddToConfig(ISteamLibrary lib, IFileSystem fs)
    {
        var steamPath = fs.Path.GetFullPath(SteamInstallPath);
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