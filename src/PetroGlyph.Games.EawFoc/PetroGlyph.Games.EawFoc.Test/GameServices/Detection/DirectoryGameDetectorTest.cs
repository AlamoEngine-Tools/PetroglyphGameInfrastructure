using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Detection.Platform;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices.Detection
{
    public class DirectoryGameDetectorTest
    {
        [Fact]
        public void TestInvalidArgs_Throws()
        {
            var fs = new MockFileSystem();
            Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null, null));
            Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), null));
        }

        [Fact]
        public void TestNoGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            var result = detector.Detect(options);
            Assert.Null(result.GameLocation);
        }

        [Fact]
        public void TestNoEawGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            var result = detector.FindGameLocation(options);
            Assert.Null(result.Location);
        }

        [Fact]
        public void TestNoFocGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Game/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.FindGameLocation(options);
            Assert.Null(result.Location);
        }

        [Fact]
        public void TestAnyEawGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Game/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            var result = detector.FindGameLocation(options);
            Assert.NotNull(result.Location);
        }

        [Fact]
        public void TestAnyFocGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.FindGameLocation(options);
            Assert.NotNull(result.Location);
        }

        [Fact]
        public void TestInKnownSubFocGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Dir/EAWX/swfoc.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Dir"), sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.FindGameLocation(options);
            Assert.NotNull(result.Location);
        }

        [Fact]
        public void TestNotInKnownSubFocGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Dir/EAWX/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Dir"), sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.FindGameLocation(options);
            Assert.Null(result.Location);
        }

        [Fact]
        public void TestInUnknownSubFocGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Dir/SomeDir/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Dir"), sp.Object);
            var options = new GameDetectorOptions(GameType.Foc);
            var result = detector.FindGameLocation(options);
            Assert.Null(result.Location);
        }

        [Fact]
        public void TestInKnownSubEawGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Dir/GameData/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Dir"), sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            var result = detector.FindGameLocation(options);
            Assert.NotNull(result.Location);
        }

        [Fact]
        public void TestInKnownSubSubEawGame()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Dir/Sub/GameData/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Dir"), sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            var result = detector.FindGameLocation(options);
            Assert.NotNull(result.Location);
        }


        [Fact]
        public void TestAnyFocGame_Integration()
        {
#if NET
            return;
#endif
            var fs = new MockFileSystem();
            fs.AddFile("Game/sweaw.exe", MockFileData.NullObject);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            var pi = new Mock<IGamePlatformIdentifier>();
            var options = new GameDetectorOptions(GameType.EaW);
            pi.Setup(i => i.GetGamePlatform(It.IsAny<GameType>(), ref It.Ref<IDirectoryInfo>.IsAny, It.IsAny<IList<GamePlatform>>()))
                .Returns(GamePlatform.GoG);
            sp.Setup(p => p.GetService(typeof(IGamePlatformIdentifier))).Returns(pi.Object);
            var detector = new DirectoryGameDetector(fs.DirectoryInfo.FromDirectoryName("Game"), sp.Object);
            var result = detector.Detect(options);
            Assert.NotNull(result.GameLocation);
            Assert.Equal(GamePlatform.GoG, result.GameIdentity.Platform);
        }
    }
}