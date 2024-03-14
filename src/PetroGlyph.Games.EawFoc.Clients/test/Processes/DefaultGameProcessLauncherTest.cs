using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Processes;

public class DefaultGameProcessLauncherTest
{
    private readonly DefaultGameProcessLauncher _service;
    private readonly Mock<IArgumentCommandLineBuilder> _builder;


    public DefaultGameProcessLauncherTest()
    {
        var sc = new ServiceCollection();
        _builder = new Mock<IArgumentCommandLineBuilder>();
        sc.AddTransient(_ => _builder.Object);
        _service = new DefaultGameProcessLauncher(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestWrapsException_Throws()
    {
        var game = new Mock<IGame>();
        var process = new GameProcessInfo(game.Object, GameBuildType.Release, ArgumentCollection.Empty);
        Assert.Throws<GameStartException>(() => _service.StartGameProcess(null, process));
    }

    [Fact]
    public void TestWrapsException_Throws1()
    {
        var fs = new MockFileSystem();
        var game = new Mock<IGame>();
        game.Setup(g => g.Game).Returns(game.Object); 
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("path"));
        var process = new GameProcessInfo(game.Object, GameBuildType.Release, ArgumentCollection.Empty);
        Assert.Throws<GameStartException>(() => _service.StartGameProcess(fs.FileInfo.New("test.exe"), process));

        _builder.Verify(b => b.BuildCommandLine(ArgumentCollection.Empty), Times.Exactly(1));
    }
}