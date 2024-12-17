using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Steam;

public class SteamPetroglyphStarWarsGameDetectorTest
{
    private readonly SteamPetroglyphStarWarsGameDetector _service;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<ISteamWrapper> _steamWrapper;
    private readonly Mock<IGameRegistryFactory> _gameRegistryFactory;
    private readonly Mock<IGameRegistry> _gameRegistry;
    private readonly Mock<ISteamLibrary> _gameLib;

    public SteamPetroglyphStarWarsGameDetectorTest()
    {
        var sc = new ServiceCollection();
        PetroglyphGameInfrastructure.InitializeServices(sc);
        _fileSystem = new MockFileSystem();
        _steamWrapper = new Mock<ISteamWrapper>();

        var steamFactory = new Mock<ISteamWrapperFactory>();
        steamFactory.Setup(f => f.CreateWrapper()).Returns(_steamWrapper.Object);
        sc.AddSingleton(steamFactory.Object);

        _gameRegistryFactory = new Mock<IGameRegistryFactory>();
        _gameRegistry = new Mock<IGameRegistry>();
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        sc.AddTransient(_ => _steamWrapper.Object);
        sc.AddTransient(_ => _gameRegistryFactory.Object);
        _service = new SteamPetroglyphStarWarsGameDetector(sc.BuildServiceProvider());
        _gameLib = new Mock<ISteamLibrary>();
    }

    [Fact]
    public void TestInvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameDetector(null!));
    }

    [Fact]
    public void TestNoGame1()
    {
        _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out It.Ref<SteamAppManifest?>.IsAny))
            .Returns(false);
        var result = _service.Detect(GameType.Foc);
        Assert.Null(result.GameLocation);
    }

    [Fact]
    public void TestNoGame2()
    {
        var installLocation = _fileSystem.DirectoryInfo.New("Game");
        var mFile = _fileSystem.FileInfo.New("manifest.acf");
        var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateInvalid,
            new HashSet<uint> { 32472 });

        _steamWrapper.Setup(s => s.Installed).Returns(true);
        _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

        var result = _service.Detect(GameType.Foc);
        Assert.Null(result.GameLocation);
    }

    [Fact]
    public void TestNoGame3()
    {
        var installLocation = _fileSystem.DirectoryInfo.New("Game");
        _fileSystem.Initialize().WithFile("Game/swfoc.exe");
        var mFile = _fileSystem.FileInfo.New("manifest.acf");
        var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateInvalid,
            new HashSet<uint> { 32472 });

        _steamWrapper.Setup(s => s.Installed).Returns(true);
        _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
        var result = _service.Detect(GameType.Foc);
        Assert.Null(result.GameLocation);
    }

    [Fact]
    public void TestNoGame4()
    {
        var installLocation = _fileSystem.DirectoryInfo.New("Game");
        _fileSystem.Initialize().WithFile("Game/swfoc.exe");
        var mFile = _fileSystem.FileInfo.New("manifest.acf");
        var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateInvalid,
            new HashSet<uint>());

        _steamWrapper.Setup(s => s.Installed).Returns(true);
        _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
        var result = _service.Detect(GameType.Foc);
        Assert.Null(result.GameLocation);
    }

    //[Fact]
    //public void TestNoGame5()
    //{
    //    var installLocation = _fileSystem.DirectoryInfo.New("Game");
    //    _fileSystem.Initialize().WithFile("Game/swfoc.exe");
    //    var mFile = _fileSystem.FileInfo.New("manifest.acf");
    //    var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
    //        new HashSet<uint> { 32472 });

    //    _steamWrapper.Setup(s => s.Installed).Returns(true);
    //    _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
    //    var result = _service.Detect(GameType.Foc);
    //    Assert.Null(result.GameLocation);
    //}

    //[Fact]
    //public void TestGameExists1()
    //{
    //    var installLocation = _fileSystem.DirectoryInfo.New("Game");
    //    _fileSystem.Initialize().WithFile("Game/corruption/swfoc.exe");
    //    var mFile = _fileSystem.FileInfo.New("manifest.acf");
    //    var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
    //        new HashSet<uint> { 32472 });

    //    _steamWrapper.Setup(s => s.Installed).Returns(true);
    //    _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

    //    _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc))
    //        .Returns(_gameRegistry.Object);
    //    _gameRegistry.Setup(r => r.Type).Returns(GameType.Foc);
    //    _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

    //    var result = _service.Detect(GameType.Foc);
    //    Assert.NotNull(result.GameLocation);
    //}

    //[Fact]
    //public void TestGameExists3()
    //{
    //    var installLocation = _fileSystem.DirectoryInfo.New("Game");
    //    _fileSystem.Initialize().WithFile("Game/corruption/swfoc.exe");
    //    var mFile = _fileSystem.FileInfo.New("manifest.acf");
    //    var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation,
    //        SteamAppState.StateFullyInstalled | SteamAppState.StateAppRunning, new HashSet<uint> { 32472 });

    //    _steamWrapper.Setup(s => s.Installed).Returns(true);
    //    _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

    //    _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc))
    //        .Returns(_gameRegistry.Object);
    //    _gameRegistry.Setup(r => r.Type).Returns(GameType.Foc);
    //    _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

    //    var result = _service.Detect(GameType.Foc);
    //    Assert.NotNull(result.GameLocation);
    //}

    //[Fact]
    //public void TestGameExists4()
    //{
    //    var installLocation = _fileSystem.DirectoryInfo.New("Game");
    //    _fileSystem.Initialize().WithFile("Game/GameData/sweaw.exe");
    //    var mFile = _fileSystem.FileInfo.New("manifest.acf");
    //    var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
    //        new HashSet<uint> { 32472 });

    //    _steamWrapper.Setup(s => s.Installed).Returns(true);
    //    _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

    //    _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Eaw))
    //        .Returns(_gameRegistry.Object);
    //    _gameRegistry.Setup(r => r.Type).Returns(GameType.Eaw);
    //    _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

    //    var result = _service.Detect(GameType.Eaw);
    //    Assert.NotNull(result.GameLocation);
    //}

    //[Fact]
    //public void TestInvalidRegistry()
    //{
    //    var installLocation = _fileSystem.DirectoryInfo.New("Game");
    //    _fileSystem.Initialize().WithFile("Game/swfoc.exe");
    //    var mFile = _fileSystem.FileInfo.New("manifest.acf");
    //    var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
    //        new HashSet<uint> { 32472 });


    //    _steamWrapper.Setup(s => s.Installed).Returns(true);
    //    _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

    //    _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc))
    //        .Returns(_gameRegistry.Object);
    //    _gameRegistry.Setup(r => r.Type).Returns(GameType.Eaw);
    //    _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

    //    var result = _service.Detect(GameType.Foc);
    //    Assert.IsType<InvalidOperationException>(result);
    //}

    [Fact]
    public void TestGameRequiresInit()
    {
        var installLocation = _fileSystem.DirectoryInfo.New("Game");
        _fileSystem.Initialize().WithFile("Game/swfoc.exe");
        var mFile = _fileSystem.FileInfo.New("manifest.acf");
        var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
            new HashSet<uint> { 32472 });

        _steamWrapper.Setup(s => s.Installed).Returns(true);
        _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

        _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc))
            .Returns(_gameRegistry.Object);
        _gameRegistry.Setup(r => r.Type).Returns(GameType.Foc);

        var result = _service.Detect(GameType.Foc);
        Assert.True(result.InitializationRequired);
    }
}