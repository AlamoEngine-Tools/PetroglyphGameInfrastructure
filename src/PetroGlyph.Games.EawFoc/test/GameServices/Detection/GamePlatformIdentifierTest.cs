using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class GamePlatformIdentifierTest
{
    [Fact]
    public void TestNullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GamePlatformIdentifier(null!));
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);
        IDirectoryInfo? nullRef = null;
        Assert.Throws<ArgumentNullException>(() => identifier.GetGamePlatform(default, ref nullRef!));
        Assert.Throws<ArgumentNullException>(() => identifier.GetGamePlatform(default, ref nullRef!, new List<GamePlatform>()));
        var fs = new MockFileSystem();
        var locRef = fs.DirectoryInfo.New("Game");
        Assert.Throws<ArgumentNullException>(() => identifier.GetGamePlatform(default, ref locRef, null!));
    }

    [Fact]
    public void TestNoInput()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);
        var fs = new MockFileSystem();
        var locRef = fs.DirectoryInfo.New("Game");
        var actual = identifier.GetGamePlatform(default, ref locRef, new List<GamePlatform>());
        Assert.Equal(GamePlatform.Undefined, actual);
    }

    [Fact]
    public void TestUndefinedWhenNoKnownGameInput()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);
        var fs = new MockFileSystem();
        var locRef = fs.DirectoryInfo.New("Game");
        var actual = identifier.GetGamePlatform(default, ref locRef);
        Assert.Equal(GamePlatform.Undefined, actual);
    }

    [Fact]
    public void TestUndefinedWhenNoRequestedGameInput()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Disk_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.SteamGold };

        var actual = identifier.GetGamePlatform(default, ref locRef, lookup);
        Assert.Equal(GamePlatform.Undefined, actual);
    }

    [Fact]
    public void TestEawDisk()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Disk_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.Disk };
        const GameType type = GameType.EaW;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Disk, actual);
    }

    [Fact]
    public void TestFocDisk()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Disk_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.Disk };
        const GameType type = GameType.Foc;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Disk, actual);
    }

    [Fact]
    public void TestEawSteam()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Steam_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.SteamGold };
        const GameType type = GameType.EaW;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.SteamGold, actual);
    }

    [Fact]
    public void TestFocSteam()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Steam_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.SteamGold };
        const GameType type = GameType.Foc;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.SteamGold, actual);
    }

    [Fact]
    public void TestEawGog()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Gog_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.GoG };
        const GameType type = GameType.EaW;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.GoG, actual);
    }

    [Fact]
    public void TestFocGog()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Gog_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.GoG };
        const GameType type = GameType.Foc;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.GoG, actual);
    }

    [Fact]
    public void TestEawGold()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = DiskGold_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.DiskGold };
        const GameType type = GameType.EaW;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.DiskGold, actual);
    }

    [Fact]
    public void TestFocGold()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = DiskGold_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.DiskGold };
        var type = GameType.Foc;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.DiskGold, actual);
    }

    [Fact]
    public void TestEawOrigin()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Origin_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.Origin };
        const GameType type = GameType.EaW;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Origin, actual);
    }

    [Fact]
    public void TestFocOrigin()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Origin_Foc_Corrected();
        var lookup = new List<GamePlatform> { GamePlatform.Origin };
        var type = GameType.Foc;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Origin, actual);
    }

    [Fact]
    public void TestFocOriginWithSanitization()
    {
        var sp = new Mock<IServiceProvider>();
        var identifier = new GamePlatformIdentifier(sp.Object);

        var locRef = Origin_Foc_Registry();
        var locStore = locRef;
        var lookup = new List<GamePlatform> { GamePlatform.Origin };
        var type = GameType.Foc;

        var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Origin, actual);
        Assert.NotEqual(locStore, locRef);
    }

    private static IDirectoryInfo Disk_Eaw()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("GameData/sweaw.exe");
        return fs.DirectoryInfo.New("GameData");
    }

    private static IDirectoryInfo Disk_Foc()
    {
        var fs = new MockFileSystem();
        fs.Initialize().WithFile("Game/swfoc.exe");
        return fs.DirectoryInfo.New("Game");
    }

    private static IFileSystem SteamFs()
    {
        var fs = new MockFileSystem();

        fs.Initialize()
            .WithFile("Game/corruption/swfoc.exe")
            .WithFile("Game/corruption/StarWarsG.exe")
            .WithFile("Game/GameData/sweaw.exe")
            .WithFile("Game/GameData/StarWarsG.exe")
            .WithFile("Game/32470_install.vdf")
            .WithFile("Game/32472_install.vdf")
            .WithFile("Game/runme.dat")
            .WithFile("Game/runm2.dat")
            .WithFile("Game/runme.exe")
            .WithFile("Game/runme2.exe");
        return fs;
    }

    private static IDirectoryInfo Steam_Eaw()
    {
        return SteamFs().DirectoryInfo.New("Game/GameData");
    }

    private static IDirectoryInfo Steam_Foc()
    {
        return SteamFs().DirectoryInfo.New("Game/corruption");
    }

    private static IFileSystem GogFs()
    {
        var fs = new MockFileSystem();
        fs.Initialize()
            .WithFile("Game/EAWX/swfoc.exe")
            .WithFile("Game/corruption/StarWarsG.exe")
            .WithFile("Game/GameData/sweaw.exe")
            .WithFile("Game/GameData/goggame-1421404887.dll")
            .WithFile("Game/goggame.sdb")
            .WithFile("Game/goggame-1421404887.hashdb")
            .WithFile("Game/goggame-1421404887.info")
            .WithFile("Game/Language.exe");
        return fs;
    }

    private static IDirectoryInfo Gog_Eaw()
    {
        return GogFs().DirectoryInfo.New("Game/GameData");
    }

    private static IDirectoryInfo Gog_Foc()
    {
        return GogFs().DirectoryInfo.New("Game/EAWX");
    }

    private static IFileSystem DiskGoldFs()
    {
        var fs = new MockFileSystem();
        fs.Initialize()
            .WithFile("Game/Foc/swfoc.exe")
            .WithFile("Game/Foc/fpupdate.exe")
            .WithFile("Game/Foc/LaunchEAWX.exe")
            .WithFile("Game/Foc/main.wav")
            .WithSubdirectory("Game/Foc/Install")
            .WithSubdirectory("Game/Foc/Manuals")

            .WithFile("Game/Eaw/GameData/sweaw.exe")
            .WithFile("Game/Eaw/GameData/fpupdate.exe")
            .WithFile("Game/Eaw/GameData/MCELaunch.exe")
            .WithFile("Game/Eaw/GameData/StubUpdate.exe")
            .WithFile("Game/Eaw/LaunchEAW.exe")
            .WithFile("Game/Eaw/main.wav");
        return fs;
    }

    private static IDirectoryInfo DiskGold_Eaw()
    {
        return DiskGoldFs().DirectoryInfo.New("Game/Eaw/GameData");
    }

    private static IDirectoryInfo DiskGold_Foc()
    {
        return DiskGoldFs().DirectoryInfo.New("Game/Foc");
    }

    private static IFileSystem OriginFs()
    {
        var fs = new MockFileSystem();
        fs.Initialize()
            .WithFile("Game/EAWX/swfoc.exe")
            .WithFile("Game/EAWX/EALaunchHelper.exe")

            .WithFile("Game/GameData/sweaw.exe")
            .WithSubdirectory("Game/Manuals")
            .WithSubdirectory("Game/__Installer");
        return fs;
    }

    private static IDirectoryInfo Origin_Eaw()
    {
        return OriginFs().DirectoryInfo.New("Game/GameData");
    }

    private static IDirectoryInfo Origin_Foc_Corrected()
    {
        return OriginFs().DirectoryInfo.New("Game/EAWX");
    }

    private static IDirectoryInfo Origin_Foc_Registry()
    {
        return OriginFs().DirectoryInfo.New("Game/corruption");
    }
}