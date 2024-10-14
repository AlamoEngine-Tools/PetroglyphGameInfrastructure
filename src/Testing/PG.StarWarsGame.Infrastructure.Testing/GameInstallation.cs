using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing;

public static partial class GameInstallation
{
    private const string EawNormalPath = "games/eaw";
    
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
        fs.Initialize().WithSubdirectory(EawNormalPath).WithFile(fs.Path.Combine(EawNormalPath, "sweaw.exe"));
        var gameDir = fs.DirectoryInfo.New(EawNormalPath);
        return gameDir;
    }

    private static IDirectoryInfo InstallEawOrigin(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }

    private static IDirectoryInfo InstallEawGog(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }

    private static IDirectoryInfo InstallEawSteam(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }

    private static IDirectoryInfo InstallEawDiskGold(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }
}

public static partial class GameInstallation
{
    private const string FocNormalPath = "games/foc";

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
        fs.Initialize().WithSubdirectory(FocNormalPath).WithFile(fs.Path.Combine(FocNormalPath, "swfoc.exe"));
        var gameDir = fs.DirectoryInfo.New(FocNormalPath);
        return gameDir;
    }

    private static IDirectoryInfo InstallFocOrigin(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }

    private static IDirectoryInfo InstallFocGog(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }

    private static IDirectoryInfo InstallFocSteam(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }

    private static IDirectoryInfo InstallFocDiskGold(this MockFileSystem fs)
    {
        throw new NotImplementedException();
    }
}

public static partial class GameInstallation
{
    private const string FocSteamPath = "";

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

    private static void InstallModsLocations(this MockFileSystem fileSystem, IDirectoryInfo directory)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(directory.FullName, "Mods"));
    }
}