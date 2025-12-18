using System;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

internal static partial class GameInstallation
{
    // Ensure starts on path 'steam'. See SteamInstallation.cs
    internal const string SteamBasePath = "steam/steamapps/common/Star Wars Empire at War";
    internal const string GogBasePath = "games/gog";
    internal const string OriginBasePath = "games/origin";

    private static void InstallDataAndMegaFilesXml(this IFileSystem fs, IDirectoryInfo directory)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(directory.FullName, "Data", "megafiles.xml"));
    }

    private static void InstallSteamFiles(this IFileSystem fs, Action initAction)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "32470_install.vdf"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "32472_install.vdf"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runme.dat"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runm2.dat"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runme.exe"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runme2.exe"));

        fs.Directory.CreateDirectory(fs.Path.Combine(SteamBasePath, "..", "..", "workshop", "content", "32470"));
        initAction();
    }

    private static void InstallGoGFiles(this IFileSystem fs, Action initAction)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(GogBasePath, "goggame.sdb"));
        CreateFile(fs, fs.Path.Combine(GogBasePath, "goggame-1421404887.hashdb"));
        CreateFile(fs, fs.Path.Combine(GogBasePath, "goggame-1421404887.info"));
        CreateFile(fs, fs.Path.Combine(GogBasePath, "Language.exe"));
        initAction();
    }

    private static void InstallOriginFiles(this IFileSystem fs, Action initAction)
    {
        fs.Initialize();
        fs.Directory.CreateDirectory(fs.Path.Combine(OriginBasePath, "Manuals"));
        fs.Directory.CreateDirectory(fs.Path.Combine(OriginBasePath, "__Installer"));
        initAction();
    }

    internal static void InstallModsLocations(this IFileSystem fileSystem, IDirectoryInfo directory)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(directory.FullName, "Mods"));
    }
    
    internal static void CreateFile(IFileSystem fs, string path)
    {
        var dir = fs.Path.GetDirectoryName(path)!;
        fs.Directory.CreateDirectory(dir);
        using var file = fs.File.Create(path);
    }
}