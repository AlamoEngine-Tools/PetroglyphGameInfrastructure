using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class CompositeDetectorTest : GameDetectorTestBase
{
    [Fact]
    public void Ctor_InvalidThrows()
    {
        Assert.Throws<ArgumentException>(() => new CompositeGameDetector([], ServiceProvider, true));
        Assert.Throws<ArgumentNullException>(() => new CompositeGameDetector([new EmptyDetector()], null!, true));
        Assert.Throws<ArgumentException>(() => new CompositeGameDetector([null!], ServiceProvider, true));
        Assert.Throws<ArgumentNullException>(() => new CompositeGameDetector(null!, ServiceProvider, true));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_InstalledGame_MultipleDetectorsRunInSpecifiedSequence(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.FromInstalled(identity, FileSystem.DirectoryInfo.New("installed"));

        IList<IGameDetector> executedDetectors = [];

        var firstDetector = new CallbackDetector((d, gt, pls) =>
        {
            executedDetectors.Add(d);
            Assert.Equal(identity.Type, gt);
            Assert.Equal([identity.Platform], pls);
            return GameDetectionResult.NotInstalled(gt);
        }, false);

        var secondDetector = new CallbackDetector((d, gt, pls) =>
        {
            executedDetectors.Add(d);
            Assert.Equal(identity.Type, gt);
            Assert.Equal([identity.Platform], pls);
            return GameDetectionResult.FromInstalled(identity, FileSystem.DirectoryInfo.New("installed"));
        }, false);

        var detector = new CompositeGameDetector(
            [firstDetector, secondDetector], 
            ServiceProvider,
            disposeDetectors: false);

        var result = detector.Detect(identity.Type, [identity.Platform]);
        Assert.Equal([firstDetector, secondDetector], executedDetectors);
        expectedResult.AssertEqual(result);
        Assert.False(firstDetector.IsDisposed);
        Assert.False(secondDetector.IsDisposed);

        executedDetectors.Clear();

        var success = detector.TryDetect(identity.Type, [identity.Platform], out result);
        Assert.True(success);
        Assert.Equal([firstDetector, secondDetector], executedDetectors);
        expectedResult.AssertEqual(result);
        Assert.False(firstDetector.IsDisposed);
        Assert.False(secondDetector.IsDisposed);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_InstalledGame_SecondDetectorDoesNotRunBecauseFirstFoundGame(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.FromInstalled(identity, FileSystem.DirectoryInfo.New("installed"));

        IList<IGameDetector> executedDetectors = [];

        var firstDetector = new CallbackDetector((d, gt, pls) =>
        {
            executedDetectors.Add(d);
            Assert.Equal(identity.Type, gt);
            Assert.Equal([identity.Platform], pls);
            return GameDetectionResult.FromInstalled(identity, FileSystem.DirectoryInfo.New("installed"));
        }, false);

        var secondDetector = new CallbackDetector((d, _, _) =>
        {
            executedDetectors.Add(d);
            Assert.Fail();
            return null!;
        }, false);

        var detector = new CompositeGameDetector([firstDetector, secondDetector], ServiceProvider);

        var result = detector.Detect(identity.Type, [identity.Platform]);
        Assert.Equal([firstDetector], executedDetectors);
        expectedResult.AssertEqual(result);

        executedDetectors.Clear();

        var success = detector.TryDetect(identity.Type, [identity.Platform], out result);
        Assert.True(success);
        Assert.Equal([firstDetector], executedDetectors);
        expectedResult.AssertEqual(result);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_InstalledGame_ShallNotThrowIfInstalled(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.FromInstalled(identity, FileSystem.DirectoryInfo.New("installed"));

        var firstDetector = new ThrowingDetector();
        var secondDetector = new InstalledDetector(identity, FileSystem);

        var detector = new CompositeGameDetector([firstDetector, secondDetector], ServiceProvider);

        var result = detector.Detect(identity.Type, [identity.Platform]);
        expectedResult.AssertEqual(result);

        var success = detector.TryDetect(identity.Type, [identity.Platform], out result);
        Assert.True(success);
        expectedResult.AssertEqual(result);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_NotInstalledGame_ShallNotThrowEvenIfNotInstalled(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.NotInstalled(identity.Type);

        var firstDetector = new ThrowingDetector();
        var secondDetector = new NotInstalledDetector();

        var detector = new CompositeGameDetector([firstDetector, secondDetector], ServiceProvider);

        var result = detector.Detect(identity.Type, [identity.Platform]);
        expectedResult.AssertEqual(result);

        var success = detector.TryDetect(identity.Type, [identity.Platform], out result);
        Assert.False(success);
        expectedResult.AssertEqual(result);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_NotInstalledGame_WhenAllDetectorsReturnNull(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.NotInstalled(identity.Type);

        var firstDetector = new NullReturningDetector();
        var secondDetector = new NullReturningDetector();

        var detector = new CompositeGameDetector([firstDetector, secondDetector], ServiceProvider);

        var result = detector.Detect(identity.Type, [identity.Platform]);
        expectedResult.AssertEqual(result);

        var success = detector.TryDetect(identity.Type, [identity.Platform], out result);
        Assert.False(success);
        expectedResult.AssertEqual(result);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_ShallThrowAggregateException_TryDetect_ReturnsNotInstalledGame_WhenAllDetectorsThrow(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.NotInstalled(identity.Type);

        var firstDetector = new ThrowingDetector();
        var secondDetector = new ThrowingDetector();

        var detector = new CompositeGameDetector([firstDetector, secondDetector], ServiceProvider);

        Assert.Throws<AggregateException>(() => detector.Detect(identity.Type, [identity.Platform]));

        var success = detector.TryDetect(identity.Type, [identity.Platform], out var result);
        Assert.False(success);
        expectedResult.AssertEqual(result);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_ShallPropagateRequireInitializationEvent(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.RequiresInitialization(identity.Type);

        var firstDetector = new CallbackDetector(
            (d, t, _) => GameDetectionResult.RequiresInitialization(t),
            true);

        var secondDetector = new CallbackDetector(
            (d, t, _) => GameDetectionResult.RequiresInitialization(t),
            true);

        var detector = new CompositeGameDetector([firstDetector, secondDetector], ServiceProvider);


        var count = 0;
        detector.InitializationRequested += (sender, args) =>
        {
            Assert.Equal(identity.Type, args.GameType);
            Assert.Same(detector, sender);
            count++;
        };

        var result = detector.Detect(identity.Type, [identity.Platform]);
        Assert.Equal(2, count);
        expectedResult.AssertEqual(result);

        // Reset
        count = 0;

        var success = detector.TryDetect(identity.Type, [identity.Platform], out result);
        Assert.Equal(2, count);
        Assert.False(success);
        expectedResult.AssertEqual(result);
    }

    //[Fact]
    //public void TestDetectRaise()
    //{
    //    var sp = new Mock<IServiceProvider>();
    //    var innerDetector = new Mock<IGameDetector>();
    //    var detector = new CompositeGameDetector(new List<IGameDetector> { innerDetector.Object }, sp.Object);
    //    var options = new GameDetectorOptions(GameType.Eaw);
    //    innerDetector
    //        .Setup(i => i.Detect(options)).Returns(new GameDetectionResult(GameType.Eaw, new Exception()))
    //        .Raises(d => d.InitializationRequested += null, this, new GameInitializeRequestEventArgs(options));

    //    var eventRaised = false;
    //    detector.InitializationRequested += (_, _) => eventRaised = true;
    //    detector.Detect(options);
    //    Assert.True(eventRaised);
    //}

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_ShallDisposeDetectors(GameIdentity identity)
    {
        var expectedResult = GameDetectionResult.FromInstalled(identity, FileSystem.DirectoryInfo.New("installed"));

        var firstDetector = new NotInstalledDetector();
        var secondDetector = new ThrowingDetector();
        var thirdDetector = new InstalledDetector(identity, FileSystem);

        var detector = new CompositeGameDetector(
            [firstDetector, secondDetector, thirdDetector], 
            ServiceProvider, 
            disposeDetectors: true);

        var result = detector.Detect(identity.Type, [identity.Platform]);
        expectedResult.AssertEqual(result);
        Assert.True(firstDetector.IsDisposed);
        Assert.True(secondDetector.IsDisposed);
        Assert.True(thirdDetector.IsDisposed);
    }

    private class EmptyDetector : IGameDetector
    {
        public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;
        public GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms) => throw new NotImplementedException();
        public bool TryDetect(GameType gameType, ICollection<GamePlatform> platforms, out GameDetectionResult result) => throw new NotImplementedException();
    }

    private delegate GameDetectionResult DetectDelegate(IGameDetector detector, GameType gameType, ICollection<GamePlatform> platforms);

    private class CallbackDetector(DetectDelegate action, bool shallRaiseEvent) : TestDetectorBase
    {
        public override GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms)
        {
            var result = action(this, gameType, platforms);
            if (shallRaiseEvent)
                OnInitializationRequested(new GameInitializeRequestEventArgs(gameType));
            return result;
        }
    }

    private class InstalledDetector(GameIdentity identity, IFileSystem fileSystem) : TestDetectorBase
    {
        public override GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms)
        {
            Assert.Equal(identity.Type, gameType);
            return GameDetectionResult.FromInstalled(identity, fileSystem.DirectoryInfo.New("installed"));
        }
    }

    private class ThrowingDetector : TestDetectorBase
    {
        public override GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms)
        {
            throw new InvalidOperationException("detector error");
        }
    }

    private class NotInstalledDetector : TestDetectorBase
    {
        public override GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms)
        {
            return GameDetectionResult.NotInstalled(gameType);
        }
    }

    private class NullReturningDetector : TestDetectorBase
    {
        public override GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms) => null!;
    }

    private abstract class TestDetectorBase :  DisposableObject, IGameDetector
    {
        public event EventHandler<GameInitializeRequestEventArgs>? InitializationRequested;

        public abstract GameDetectionResult Detect(GameType gameType, ICollection<GamePlatform> platforms);

        public bool TryDetect(GameType gameType, ICollection<GamePlatform> platforms, out GameDetectionResult result)
        {
            try
            {
                result = Detect(gameType, platforms);
                return true;
            }
            catch
            {
                result = GameDetectionResult.NotInstalled(gameType);
                return false;
            }
        }

        protected virtual void OnInitializationRequested(GameInitializeRequestEventArgs e)
        {
            InitializationRequested?.Invoke(this, e);
        }
    }
}