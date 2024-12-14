using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class DirectoryGameDetectorTest : GameDetectorTestBase<EmptyStruct>
{
    protected override bool SupportInitialization => false;

    protected override IGameDetector CreateDetector(GameDetectorTestInfo<EmptyStruct> gameInfo, bool shallHandleInitialization)
    {
        return new DirectoryGameDetector(gameInfo.GameDirectory ?? FileSystem.DirectoryInfo.New("doesNotExist"), ServiceProvider);
    }

    protected override GameDetectorTestInfo<EmptyStruct> SetupGame(GameIdentity gameIdentity)
    {
        var game = FileSystem.InstallGame(gameIdentity, ServiceProvider);
        return new(gameIdentity.Type, game.Directory, default);
    }

    [Fact]
    public void InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null!, null!));
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(FileSystem.DirectoryInfo.New("Game"), null!));
        Assert.Throws<ArgumentNullException>(() => new DirectoryGameDetector(null!, ServiceProvider));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GamesNotInstalled_DirectoryNotFound(GameIdentity identity)
    {
        TestNotInstalledWithCustomSetup(identity, _ => FileSystem.DirectoryInfo.New("doesNotExist"));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GamesNotInstalled_GameExeNotFound(GameIdentity identity)
    {
        FileSystem.Initialize().WithFile("Game/Data/megafiles.xml");
        TestNotInstalledWithCustomSetup(identity, _ => FileSystem.DirectoryInfo.New("Game"));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GamesNotInstalled_GameDataWithMegaFilesNotFound(GameIdentity identity)
    {
        var exeName = identity.Type == GameType.Eaw
            ? PetroglyphStarWarsGameConstants.EmpireAtWarExeFileName
            : PetroglyphStarWarsGameConstants.ForcesOfCorruptionExeFileName;

        FileSystem.Initialize().WithSubdirectory("Game").WithFile($"Game/{exeName}");
        TestNotInstalledWithCustomSetup(identity, _ => FileSystem.DirectoryInfo.New("doesNotExist"));

        FileSystem.Directory.CreateDirectory("Game/Data");
        TestNotInstalledWithCustomSetup(identity, _ => FileSystem.DirectoryInfo.New("doesNotExist"));
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Detect_TryDetect_GamesInstalled_WrongPathGiven(GameIdentity identity)
    {
        TestNotInstalledWithCustomSetup(identity, i =>
        {
            FileSystem.InstallGame(i, ServiceProvider);
            return FileSystem.DirectoryInfo.New("other");
        });
    }

    private void TestNotInstalledWithCustomSetup(GameIdentity identity, Func<GameIdentity, IDirectoryInfo> customSetup)
    {
        var expected = GameDetectionResult.NotInstalled(identity.Type);
        TestDetectorCore(
            identity,
            false,
            _ => new GameDetectorTestInfo<EmptyStruct>(identity.Type, customSetup(identity), default),
            _ => expected,
            null,
            queryPlatforms: []);
    }

    protected override GameDetectorTestInfo<EmptyStruct> SetupForRequiredInitialization(GameIdentity identity)
        => throw new NotSupportedException();

    protected override void HandleInitialization(bool shallInitSuccessfully, GameDetectorTestInfo<EmptyStruct> info)
        => throw new NotSupportedException();
}