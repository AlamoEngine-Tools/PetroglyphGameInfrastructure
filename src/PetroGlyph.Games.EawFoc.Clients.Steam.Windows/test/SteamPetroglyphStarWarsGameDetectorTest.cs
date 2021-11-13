using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Registry;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Windows.Test
{
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
            _fileSystem = new MockFileSystem();
            _steamWrapper = new Mock<ISteamWrapper>();
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
            Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameDetector(null));
        }

        [Fact]
        public void TestNoGame1()
        {
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out It.Ref<SteamAppManifest?>.IsAny))
                .Returns(false);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame2()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateInvalid,
                new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame3()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateInvalid,
                new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame4()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateInvalid,
                new HashSet<uint>());

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame5()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
                new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestGameExists1()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/corruption/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
                new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

            _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc, It.IsAny<IServiceProvider>()))
                .Returns(_gameRegistry.Object);
            _gameRegistry.Setup(r => r.Type).Returns(GameType.Foc);
            _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestGameExists3()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/corruption/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation,
                SteamAppState.StateFullyInstalled | SteamAppState.StateAppRunning, new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

            _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc, It.IsAny<IServiceProvider>()))
                .Returns(_gameRegistry.Object);
            _gameRegistry.Setup(r => r.Type).Returns(GameType.Foc);
            _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestGameExists4()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/GameData/sweaw.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
                new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

            _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.EaW, It.IsAny<IServiceProvider>()))
                .Returns(_gameRegistry.Object);
            _gameRegistry.Setup(r => r.Type).Returns(GameType.EaW);
            _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

            var options = new GameDetectorOptions(GameType.EaW);
            var result = _service.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestInvalidRegistry()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
                new HashSet<uint> { 32472 });


            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

            _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc, It.IsAny<IServiceProvider>()))
                .Returns(_gameRegistry.Object);
            _gameRegistry.Setup(r => r.Type).Returns(GameType.EaW);
            _gameRegistry.Setup(r => r.Version).Returns(new Version(1, 0, 0, 0));

            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.IsType<InvalidOperationException>(result.Error);
        }

        [Fact]
        public void TestGameRequiresInit()
        {
            var installLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Game");
            _fileSystem.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(_gameLib.Object, mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled,
                new HashSet<uint> { 32472 });

            _steamWrapper.Setup(s => s.Installed).Returns(true);
            _steamWrapper.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);

            _gameRegistryFactory.Setup(f => f.CreateRegistry(GameType.Foc, It.IsAny<IServiceProvider>()))
                .Returns(_gameRegistry.Object);
            _gameRegistry.Setup(r => r.Type).Returns(GameType.Foc);

            var options = new GameDetectorOptions(GameType.Foc);
            var result = _service.Detect(options);
            Assert.True(result.InitializationRequired);
        }
    }
}
