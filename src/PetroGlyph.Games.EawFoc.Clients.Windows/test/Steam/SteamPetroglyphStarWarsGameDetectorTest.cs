using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Registry;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Windows.Test.Steam
{
    public class SteamPetroglyphStarWarsGameDetectorTest
    {
        [Fact]
        public void TestInvalidCtor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameDetector(null, null, null, null));
            var sp = new Mock<IServiceProvider>();
            Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameDetector(null, null, null, sp.Object));
            var steam = new Mock<ISteamWrapper>();
            Assert.Throws<ArgumentNullException>(() => new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, null));
        }

        [Fact]
        public void TestNoGame1()
        {
#if NET
            return;
#endif
            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out It.Ref<SteamAppManifest?>.IsAny)).Returns(false);
            var fs = new MockFileSystem();
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame2()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateInvalid, new HashSet<uint>{ 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame3()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateInvalid, new HashSet<uint>{ 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame4()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateInvalid, new HashSet<uint>());

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoGame5()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled, new HashSet<uint> { 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestGameExists1()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/corruption/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled, new HashSet<uint> { 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestGameExists2()
        {
#if NET
            return;
#endif
            var reg = new Mock<IGameRegistry>();
            reg.Setup(r => r.Type).Returns(GameType.Foc);
            reg.Setup(r => r.Version).Returns(new Version(1, 0));

            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/corruption/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled, new HashSet<uint> { 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, reg.Object, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestGameExists3()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/corruption/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled | SteamAppState.StateAppRunning, new HashSet<uint> { 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestGameExists4()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/GameData/sweaw.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled, new HashSet<uint> { 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, null, sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            var result = detector.Detect(options);
            Assert.NotNull(result.GameLocation);
        }

        [Fact]
        public void TestInvalidRegistry()
        {
#if NET
            return;
#endif
            var reg = new Mock<IGameRegistry>();
            reg.Setup(r => r.Type).Returns(GameType.EaW);
            reg.Setup(r => r.Version).Returns(new Version(1, 0));

            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled, new HashSet<uint> { 32472 });


            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, reg.Object, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);

            var result = detector.Detect(options);
            Assert.IsType<InvalidOperationException>(result.Error);
        }

        [Fact]
        public void TestGameRequiresInit()
        {
#if NET
            return;
#endif
            var reg = new Mock<IGameRegistry>();
            reg.Setup(r => r.Type).Returns(GameType.Foc);
            reg.Setup(r => r.Version).Returns((Version)null);

            var fs = new MockFileSystem();
            var installLocation = fs.DirectoryInfo.FromDirectoryName("Game");
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var mFile = fs.FileInfo.FromFileName("manifest.acf");
            var app = new SteamAppManifest(mFile, 1234, "name", installLocation, SteamAppState.StateFullyInstalled, new HashSet<uint> { 32472 });

            var steam = new Mock<ISteamWrapper>();
            steam.Setup(s => s.Installed).Returns(true);
            steam.Setup(s => s.IsGameInstalled(It.IsAny<uint>(), out app)).Returns(true);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new SteamPetroglyphStarWarsGameDetector(steam.Object, null, reg.Object, sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.Detect(options);
            Assert.True(result.InitializationRequired);
        }
    }
}
