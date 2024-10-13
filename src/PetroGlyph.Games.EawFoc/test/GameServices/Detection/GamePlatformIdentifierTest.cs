using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection.Platform;
using PG.StarWarsGame.Infrastructure.Testing;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class GamePlatformIdentifierTest
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly GamePlatformIdentifier _platformIdentifier;
    private readonly IServiceProvider _serviceProvider;

    public GamePlatformIdentifierTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _platformIdentifier = new GamePlatformIdentifier(_serviceProvider);
    }

    [Fact]
    public void NullArgs_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GamePlatformIdentifier(null!));

        IDirectoryInfo? nullRef = null;
        Assert.Throws<ArgumentNullException>(() => _platformIdentifier.GetGamePlatform(default, ref nullRef!, new List<GamePlatform>()));
        var locRef = _fileSystem.DirectoryInfo.New("Game");
        Assert.Throws<ArgumentNullException>(() => _platformIdentifier.GetGamePlatform(default, ref locRef, null!));
    }

    [Theory]
    [InlineData]
    [InlineData(GamePlatform.Undefined)]
    [InlineData(GamePlatform.SteamGold, GamePlatform.Undefined)]
    public void GetGamePlatform_LookupNormalizesToDefaultSearchOrder(params GamePlatform[] lookup)
    {
        // Using GOG here as it's more special than Disk, but also does not change loc ref. Steam is a param of this test, that should be overwritten.
        var game = _fileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.GoG), _serviceProvider);
        var gameLocation = game.Directory;

        var actual = _platformIdentifier.GetGamePlatform(GameType.Foc, ref gameLocation, lookup);
        Assert.Equal(GamePlatform.GoG, actual);
        Assert.Equal(game.Directory.FullName, gameLocation.FullName);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void GetGamePlatform_WrongGameInstalledReturnsUndefined(GameType queryGameType)
    {
        foreach (GamePlatform platform in Enum.GetValues(typeof(GamePlatform)))
        {
            var installType = queryGameType == GameType.Foc ? GameType.Eaw : GameType.Foc;
            var game = _fileSystem.InstallGame(new GameIdentity(installType, platform), _serviceProvider);
            var gameLocation = game.Directory;

            var actual = _platformIdentifier.GetGamePlatform(GameType.Foc, ref gameLocation, []);
            Assert.Equal(GamePlatform.Undefined, actual);
            Assert.Equal(game.Directory.FullName, gameLocation.FullName);
        }
    }


    [Fact]
    public void GetGamePlatform_NoGameInstalledReturnsUndefined()
    {
        var gameLocation = _fileSystem.DirectoryInfo.New("noGameDir");
        var locRef = gameLocation;
        var actual = _platformIdentifier.GetGamePlatform(default, ref locRef, []);
        Assert.Equal(GamePlatform.Undefined, actual);
        Assert.Equal(gameLocation.FullName, locRef.FullName);
    }

    [Fact]
    public void TestEawDisk()
    {
        var locRef = Disk_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.Disk };
        const GameType type = GameType.Eaw;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Disk, actual);
    }

    [Fact]
    public void TestFocDisk()
    {
        var locRef = Disk_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.Disk };
        const GameType type = GameType.Foc;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Disk, actual);
    }

    [Fact]
    public void TestEawSteam()
    { 
        var locRef = Steam_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.SteamGold };
        const GameType type = GameType.Eaw;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.SteamGold, actual);
    }

    [Fact]
    public void TestFocSteam()
    {
        var locRef = Steam_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.SteamGold };
        const GameType type = GameType.Foc;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.SteamGold, actual);
    }

    [Fact]
    public void TestEawGog()
    {
        var locRef = Gog_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.GoG };
        const GameType type = GameType.Eaw;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.GoG, actual);
    }

    [Fact]
    public void TestFocGog()
    {
        var locRef = Gog_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.GoG };
        const GameType type = GameType.Foc;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.GoG, actual);
    }

    [Fact]
    public void TestEawGold()
    {
        var locRef = DiskGold_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.DiskGold };
        const GameType type = GameType.Eaw;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.DiskGold, actual);
    }

    [Fact]
    public void TestFocGold()
    { 
        var locRef = DiskGold_Foc();
        var lookup = new List<GamePlatform> { GamePlatform.DiskGold };
        var type = GameType.Foc;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.DiskGold, actual);
    }

    [Fact]
    public void TestEawOrigin()
    {
        var locRef = Origin_Eaw();
        var lookup = new List<GamePlatform> { GamePlatform.Origin };
        const GameType type = GameType.Eaw;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Origin, actual);
    }

    [Fact]
    public void TestFocOrigin()
    {
        var locRef = Origin_Foc_Corrected();
        var lookup = new List<GamePlatform> { GamePlatform.Origin };
        var type = GameType.Foc;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
        Assert.Equal(GamePlatform.Origin, actual);
    }

    [Fact]
    public void TestFocOriginWithSanitization()
    { 
        var locRef = Origin_Foc_Registry();
        var locStore = locRef;
        var lookup = new List<GamePlatform> { GamePlatform.Origin };
        var type = GameType.Foc;

        var actual = _platformIdentifier.GetGamePlatform(type, ref locRef, lookup);
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