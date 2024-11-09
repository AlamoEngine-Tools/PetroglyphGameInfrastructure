using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.Initializer;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

public static partial class GameInstallation
{
    private const string SteamBasePath = "steam/apps/common/SteamSW";
    private const string GogBasePath = "games/gog";
    private const string OriginBasePath = "games/origin";

    public static IGame InstallGame(this MockFileSystem fs, GameIdentity gameIdentity, IServiceProvider sp)
    { 
        Func<MockFileSystem, GamePlatform, IDirectoryInfo> installFunc;

        if (gameIdentity.Type == GameType.Foc)
            installFunc = InstallFoc;
        else
            installFunc = InstallEaw;

        var gameDir = installFunc(fs, gameIdentity.Platform);

        fs.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), sp);
        Assert.True(game.Exists());
        return game;
    }

    private static void InstallDataAndMegaFilesXml(this MockFileSystem fs, IDirectoryInfo directory)
    {
        fs.Initialize().WithFile(fs.Path.Combine(directory.FullName, "Data", "megafiles.xml"));
    }

    private static void InstallSteamFiles(this MockFileSystem fs, Action<IFileSystemInitializer<MockFileSystem>> initAction)
    {
        var initializer = fs.Initialize()
            .WithFile(fs.Path.Combine(SteamBasePath, "32470_install.vdf"))
            .WithFile(fs.Path.Combine(SteamBasePath, "32472_install.vdf"))
            .WithFile(fs.Path.Combine(SteamBasePath, "runme.dat"))
            .WithFile(fs.Path.Combine(SteamBasePath, "runm2.dat"))
            .WithFile(fs.Path.Combine(SteamBasePath, "runme.exe"))
            .WithFile(fs.Path.Combine(SteamBasePath, "runme2.exe"));

        fs.Directory.CreateDirectory(fs.Path.Combine(SteamBasePath, "..", "..", "workshop", "content", "32470"));

        initAction(initializer);
    }

    private static void InstallGoGFiles(this MockFileSystem fs, Action<IFileSystemInitializer<MockFileSystem>> initAction)
    {
        var initializer = fs.Initialize()
            .WithFile(fs.Path.Combine(GogBasePath, "goggame.sdb"))
            .WithFile(fs.Path.Combine(GogBasePath, "goggame-1421404887.hashdb"))
            .WithFile(fs.Path.Combine(GogBasePath, "goggame-1421404887.info"))
            .WithFile(fs.Path.Combine(GogBasePath, "Language.exe"));
        initAction(initializer);
    }

    private static void InstallOriginFiles(this MockFileSystem fs, Action<IFileSystemInitializer<MockFileSystem>> initAction)
    {
        var initializer = fs.Initialize()
            .WithSubdirectories(fs.Path.Combine(OriginBasePath, "Manuals"))
            .WithSubdirectories(fs.Path.Combine(OriginBasePath, "__Installer"));
        initAction(initializer);
    }

    private static void InstallModsLocations(this MockFileSystem fileSystem, IDirectoryInfo directory)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(directory.FullName, "Mods"));
    }

    private static void Clean(MockFileSystem fs, string path)
    {
        if (fs.Directory.Exists(path))
            fs.Directory.Delete(path, true);
    }
}