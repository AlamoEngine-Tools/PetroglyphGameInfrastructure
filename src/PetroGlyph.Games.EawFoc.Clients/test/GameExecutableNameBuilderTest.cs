using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test;

public class GameExecutableNameBuilderTest
{
    private readonly GameExecutableNameBuilder _service;

    public GameExecutableNameBuilderTest()
    {
        _service = new GameExecutableNameBuilder();
    }

    [Fact]
    public void TestNames()
    {
        var eaw = new Mock<IGame>();
        eaw.Setup(g => g.Type).Returns(GameType.Eaw);
        eaw.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        var foc = new Mock<IGame>();
        foc.Setup(g => g.Type).Returns(GameType.Foc);
        foc.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        var eawSteam = new Mock<IGame>();
        eawSteam.Setup(g => g.Type).Returns(GameType.Eaw);
        eawSteam.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);

        var focSteam = new Mock<IGame>();
        focSteam.Setup(g => g.Type).Returns(GameType.Foc);
        focSteam.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);

        Assert.Equal("sweaw.exe", _service.GetExecutableFileName(eaw.Object, GameBuildType.Release));
        Assert.Equal("swfoc.exe", _service.GetExecutableFileName(foc.Object, GameBuildType.Release));

        Assert.Equal("StarWarsG.exe", _service.GetExecutableFileName(eawSteam.Object, GameBuildType.Release));
        Assert.Equal("StarWarsG.exe", _service.GetExecutableFileName(focSteam.Object, GameBuildType.Release));

        Assert.Equal("StarWarsI.exe", _service.GetExecutableFileName(eawSteam.Object, GameBuildType.Debug));
        Assert.Equal("StarWarsI.exe", _service.GetExecutableFileName(focSteam.Object, GameBuildType.Debug));
    }
}