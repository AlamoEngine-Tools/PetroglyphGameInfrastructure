using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

public static partial class GameInstallation
{
    private const string FocNormalPath = "games/foc";
    private const string FocGoldPath = "games/gold/foc";
    private const string FocSteamSubPath = "corruption";
    private const string FocGogSubPath = "EAWX";
    private const string FocOriginSubPath = "EAWX";

    public static IDirectoryInfo GetWrongOriginFocRegistryLocation(this IFileSystem fs)
    {
        return fs.DirectoryInfo.New(fs.Path.Combine(OriginBasePath, "corruption"));
    }

    public static IDirectoryInfo InstallFoc(MockFileSystem fs, GamePlatform platform)
    {
        IDirectoryInfo gameDirectory;
        switch (platform)
        {
            case GamePlatform.Disk:
                gameDirectory = fs.InstallFocDisk();
                break;
            case GamePlatform.DiskGold:
                gameDirectory = fs.InstallFocDiskGold();
                break;
            case GamePlatform.SteamGold:
                gameDirectory = fs.InstallFocSteam();
                break;
            case GamePlatform.GoG:
                gameDirectory = fs.InstallFocGog();
                break;
            case GamePlatform.Origin:
                gameDirectory = fs.InstallFocOrigin();
                break;
            case GamePlatform.Undefined:
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
        fs.InstallDataAndMegaFilesXml(gameDirectory);
        return gameDirectory;
    }

    private static IDirectoryInfo InstallFocDisk(this MockFileSystem fs)
    {
        Clean(fs, FocNormalPath);
        fs.Initialize()
            .WithSubdirectory(FocNormalPath)
            .WithFile(fs.Path.Combine(FocNormalPath, "swfoc.exe"));
        var gameDir = fs.DirectoryInfo.New(FocNormalPath);
        return gameDir;
    }

    private static IDirectoryInfo InstallFocOrigin(this MockFileSystem fs)
    {
        Clean(fs, OriginBasePath);

        var basePath = fs.Path.Combine(OriginBasePath, FocOriginSubPath);

        fs.InstallOriginFiles(init =>
        {
            init.WithFile(fs.Path.Combine(basePath, "swfoc.exe"));
            init.WithFile(fs.Path.Combine(basePath, "EALaunchHelper.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallFocGog(this MockFileSystem fs)
    {
        Clean(fs, GogBasePath);

        var basePath = fs.Path.Combine(GogBasePath, FocGogSubPath);

        fs.InstallGoGFiles(init =>
        {
            init.WithFile(fs.Path.Combine(basePath, "swfoc.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallFocSteam(this MockFileSystem fs)
    {
        Clean(fs, SteamBasePath);

        var basePath = fs.Path.Combine(SteamBasePath, FocSteamSubPath);

        fs.InstallSteamFiles(init =>
        {
            init.WithFile(fs.Path.Combine(basePath, "swfoc.exe"))
                .WithFile(fs.Path.Combine(basePath, "StarWarsG.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallFocDiskGold(this MockFileSystem fs)
    {
        Clean(fs, FocGoldPath);

        fs.Initialize()
            .WithFile(fs.Path.Combine(FocGoldPath, "swfoc.exe"))
            .WithFile(fs.Path.Combine(FocGoldPath, "fpupdate.exe"))
            .WithFile(fs.Path.Combine(FocGoldPath, "LaunchEAWX.exe"))
            .WithFile(fs.Path.Combine(FocGoldPath, "main.wav"))
            .WithSubdirectories(
                fs.Path.Combine(FocGoldPath, "Install"),
                fs.Path.Combine(FocGoldPath, "Manuals"));

        return fs.DirectoryInfo.New(FocGoldPath);
    }
}