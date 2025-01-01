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
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(FocNormalPath, "swfoc.exe"));
        var gameDir = fs.DirectoryInfo.New(FocNormalPath);
        return gameDir;
    }

    private static IDirectoryInfo InstallFocOrigin(this MockFileSystem fs)
    {
        var basePath = fs.Path.Combine(OriginBasePath, FocOriginSubPath);

        fs.InstallOriginFiles(() =>
        {
            CreateFile(fs, fs.Path.Combine(basePath, "swfoc.exe"));
            CreateFile(fs, fs.Path.Combine(basePath, "EALaunchHelper.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallFocGog(this MockFileSystem fs)
    {
        var basePath = fs.Path.Combine(GogBasePath, FocGogSubPath);

        fs.InstallGoGFiles(() =>
        {
            CreateFile(fs, fs.Path.Combine(basePath, "swfoc.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallFocSteam(this MockFileSystem fs)
    {
        var basePath = fs.Path.Combine(SteamBasePath, FocSteamSubPath);

        fs.InstallSteamFiles(() =>
        {
            CreateFile(fs, fs.Path.Combine(basePath, "swfoc.exe"));
            CreateFile(fs, fs.Path.Combine(basePath, "StarWarsG.exe"));
        });
        return fs.DirectoryInfo.New(basePath);
    }

    private static IDirectoryInfo InstallFocDiskGold(this MockFileSystem fs)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(FocGoldPath, "swfoc.exe"));
        CreateFile(fs, fs.Path.Combine(FocGoldPath, "fpupdate.exe"));
        CreateFile(fs, fs.Path.Combine(FocGoldPath, "LaunchEAWX.exe"));
        CreateFile(fs, fs.Path.Combine(FocGoldPath, "main.wav"));

        fs.Directory.CreateDirectory(fs.Path.Combine(FocGoldPath, "Install"));
        fs.Directory.CreateDirectory(fs.Path.Combine(FocGoldPath, "Manuals"));

        return fs.DirectoryInfo.New(FocGoldPath);
    }
}