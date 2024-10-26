using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

public static partial class GameInstallation
{
    private const string EawNormalPath = "games/eaw";
    private const string EawGoldPath = "games/gold/eaw";
    private const string EawGameDataSubPath = "GameData";

    private static IDirectoryInfo InstallEaw(this MockFileSystem fs, GamePlatform platform)
    {
        IDirectoryInfo gameDirectory;
        switch (platform)
        {
            case GamePlatform.Disk:
                gameDirectory = fs.InstallEawDisk();
                break;
            case GamePlatform.DiskGold:
                gameDirectory = fs.InstallEawDiskGold();
                break;
            case GamePlatform.SteamGold:
                gameDirectory = fs.InstallEawSteam();
                break;
            case GamePlatform.GoG:
                gameDirectory = fs.InstallEawGog();
                break;
            case GamePlatform.Origin:
                gameDirectory = fs.InstallEawOrigin();
                break;
            case GamePlatform.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
        fs.InstallDataAndMegaFilesXml(gameDirectory);
        return gameDirectory;
    }

    private static IDirectoryInfo InstallEawDisk(this MockFileSystem fs)
    {
        Clean(fs, EawNormalPath);

        fs.Initialize().WithSubdirectory(EawNormalPath).WithFile(fs.Path.Combine(EawNormalPath, "sweaw.exe"));
        var gameDir = fs.DirectoryInfo.New(EawNormalPath);
        return gameDir;
    }

    private static IDirectoryInfo InstallEawOrigin(this MockFileSystem fs)
    {
        Clean(fs, OriginBasePath);

        var basePath = fs.Path.Combine(OriginBasePath, EawGameDataSubPath);

        fs.InstallOriginFiles(init =>
        {
            init.WithFile(fs.Path.Combine(basePath, "sweaw.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallEawGog(this MockFileSystem fs)
    {
        Clean(fs, GogBasePath);

        var basePath = fs.Path.Combine(GogBasePath, EawGameDataSubPath);

        fs.InstallGoGFiles(init =>
        {
            init.WithFile(fs.Path.Combine(basePath, "sweaw.exe"))
                .WithFile(fs.Path.Combine(basePath, "goggame-1421404887.dll"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallEawSteam(this MockFileSystem fs)
    {
        Clean(fs, SteamBasePath);

        var basePath = fs.Path.Combine(SteamBasePath, EawGameDataSubPath);

        fs.InstallSteamFiles(init =>
        {
            init.WithFile(fs.Path.Combine(basePath, "sweaw.exe"))
                .WithFile(fs.Path.Combine(basePath, "StarWarsG.exe"));
        });
        return fs.DirectoryInfo.New(fs.Path.Combine(basePath));
    }

    private static IDirectoryInfo InstallEawDiskGold(this MockFileSystem fs)
    {
        Clean(fs, EawGoldPath);

        fs.Initialize()
            .WithFile(fs.Path.Combine(EawGoldPath, EawGameDataSubPath, "sweaw.exe"))
            .WithFile(fs.Path.Combine(EawGoldPath, EawGameDataSubPath, "fpupdate.exe"))
            .WithFile(fs.Path.Combine(EawGoldPath, EawGameDataSubPath, "MCELaunch.exe"))
            .WithFile(fs.Path.Combine(EawGoldPath, EawGameDataSubPath, "StubUpdate.exe"))
            .WithFile(fs.Path.Combine(EawGoldPath, "LaunchEAW.exe"))
            .WithFile(fs.Path.Combine(EawGoldPath, "main.wav"));

        return fs.DirectoryInfo.New(fs.Path.Combine(EawGoldPath, EawGameDataSubPath));
    }
}