﻿using System;
using System.Collections.Generic;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class CompositeDetectorTest
{
    [Fact]
    public void TestInvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeGameDetector(null, null));
        Assert.Throws<ArgumentNullException>(() => new CompositeGameDetector(new List<IGameDetector> { null }, null));
        Assert.Throws<ArgumentException>(() => new CompositeGameDetector(new List<IGameDetector>(), new Mock<IServiceProvider>().Object));
    }

    [Fact]
    public void TestDetectWithError()
    {
        var sp = new Mock<IServiceProvider>();
        var innerDetector = new Mock<IGameDetector>();
        var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetector.Object }, sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        innerDetector.Setup(i => i.Detect(options)).Returns(new GameDetectionResult(GameType.Eaw, new Exception()));
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
        var options = new GameDetectorOptions(GameType.Eaw);
        innerDetector
            .Setup(i => i.Detect(options)).Returns(new GameDetectionResult(GameType.Eaw, new Exception()))
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
        var options = new GameDetectorOptions(GameType.Eaw);
        innerDetector.Setup(i => i.Detect(options))
            .Returns(
                new GameDetectionResult(new GameIdentity(GameType.Eaw, GamePlatform.Disk),
                    fs.DirectoryInfo.New("Game")));
        var result = detector.Detect(options);
        Assert.Equal(fs.Path.GetFullPath("Game"), result.GameLocation?.FullName);
    }

    [Fact]
    public void TestDetectSecond()
    {
        var fs = new MockFileSystem();
        var sp = new Mock<IServiceProvider>();
        var innerDetectorA = new Mock<IGameDetector>();
        var innerDetectorB = new Mock<IGameDetector>();
        var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetectorA.Object, innerDetectorB.Object }, sp.Object);
        var options = new GameDetectorOptions(GameType.Eaw);
        innerDetectorA.Setup(i => i.Detect(options)).Returns(GameDetectionResult.NotInstalled(GameType.Eaw));
        innerDetectorB.Setup(i => i.Detect(options))
            .Returns(
                new GameDetectionResult(new GameIdentity(GameType.Eaw, GamePlatform.Disk),
                    fs.DirectoryInfo.New("Game")));
        var result = detector.Detect(options);
        Assert.Equal(fs.Path.GetFullPath("Game"), result.GameLocation?.FullName);
    }
}