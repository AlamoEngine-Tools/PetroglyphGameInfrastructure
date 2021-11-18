using Moq;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test;

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
        eaw.Setup(g => g.Type).Returns(GameType.EaW);
        eaw.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        var foc = new Mock<IGame>();
        foc.Setup(g => g.Type).Returns(GameType.Foc);
        foc.Setup(g => g.Platform).Returns(GamePlatform.Disk);

        var eawSteam = new Mock<IGame>();
        eawSteam.Setup(g => g.Type).Returns(GameType.EaW);
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