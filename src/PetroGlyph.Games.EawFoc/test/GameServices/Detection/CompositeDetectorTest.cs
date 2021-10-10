using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices.Detection
{
    public class CompositeDetectorTest
    {
        [Fact]
        public void TestInvalidArgs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CompositeGameDetector(null, null));
            Assert.Throws<ArgumentNullException>(() => new CompositeGameDetector(new List<IGameDetector> { null }, null));
            Assert.Throws<ArgumentException>(() => new CompositeGameDetector(new List<IGameDetector>(), null));
        }

        [Fact]
        public void TestDetectWithError()
        {
            var sp = new Mock<IServiceProvider>();
            var innerDetector = new Mock<IGameDetector>();
            var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetector.Object }, sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            innerDetector.Setup(i => i.Detect(options)).Returns(new GameDetectionResult(GameType.EaW, new Exception()));
            var success = detector.TryDetect(options, out var result);
            Assert.False(success);
            Assert.IsType<AggregateException>(result.Error);
        }

        [Fact]
        public void TestDetectRaise()
        {
            var sp = new Mock<IServiceProvider>();
            var innerDetector = new Mock<IGameDetector>();
            var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetector.Object }, sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            innerDetector
                .Setup(i => i.Detect(options)).Returns(new GameDetectionResult(GameType.EaW, new Exception()))
                .Raises(d => d.InitializationRequested += null, this, new GameInitializeRequestEventArgs(options));

            var eventRaised = false;
            detector.InitializationRequested += (_, _) => eventRaised = true;
            detector.Detect(options);
            Assert.True(eventRaised);
        }

        [Fact]
        public void TestDetect()
        {
            var fs = new MockFileSystem();
            var sp = new Mock<IServiceProvider>();
            var innerDetector = new Mock<IGameDetector>();
            var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetector.Object }, sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            innerDetector.Setup(i => i.Detect(options))
                .Returns(
                    new GameDetectionResult(new GameIdentity(GameType.EaW, GamePlatform.Disk),
                        fs.DirectoryInfo.FromDirectoryName("Game")));
            var result = detector.Detect(options);
            Assert.Equal(TestUtils.IsUnixLikePlatform ? "/Game" : "C:\\Game", result.GameLocation?.FullName);
        }

        [Fact]
        public void TestDetectSecond()
        {
            var fs = new MockFileSystem();
            var sp = new Mock<IServiceProvider>();
            var innerDetectorA = new Mock<IGameDetector>();
            var innerDetectorB = new Mock<IGameDetector>();
            var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetectorA.Object, innerDetectorB.Object }, sp.Object);
            var options = new GameDetectorOptions(GameType.EaW);
            innerDetectorA.Setup(i => i.Detect(options)).Returns(GameDetectionResult.NotInstalled(GameType.EaW));
            innerDetectorB.Setup(i => i.Detect(options))
                .Returns(
                    new GameDetectionResult(new GameIdentity(GameType.EaW, GamePlatform.Disk),
                        fs.DirectoryInfo.FromDirectoryName("Game")));
            var result = detector.Detect(options);
            Assert.Equal(TestUtils.IsUnixLikePlatform ? "/Game" : "C:\\Game", result.GameLocation?.FullName);
        }
    }
}
