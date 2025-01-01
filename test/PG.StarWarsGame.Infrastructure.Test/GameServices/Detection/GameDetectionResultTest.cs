using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices.Detection;

public class GameDetectionResultTest : CommonTestBase
{
    [Fact]
    public void CreateInstance_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GameDetectionResult.FromInstalled(null!, FileSystem.DirectoryInfo.New("path")));
        Assert.Throws<ArgumentNullException>(() =>
            GameDetectionResult.FromInstalled(new GameIdentity(GameType.Eaw, GamePlatform.Disk), null!));
    }
}