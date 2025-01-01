using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Games;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Processes;

public class GameProcessTest
{
    [Fact]
    public async Task TestOnExit()
    {
        var game = new Mock<IGame>();

        var processName = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bash" : "cmd";

        var p = Process.Start(processName);
        var gp = new GameProcess(p, new GameProcessInfo(game.Object, GameBuildType.Debug, ArgumentCollection.Empty));
        var tcs = new TaskCompletionSource<bool>();
            
        gp.Closed += (_, _) => {
            tcs.SetResult(true);
        };
        p.Kill();
        var cts = new CancellationTokenSource(5000);
        using (cts.Token.Register(() => tcs.TrySetCanceled(cts.Token)))
            Assert.True(await tcs.Task);
    }

    [Fact]
    public async Task TestAlreadyExited()
    {
        var game = new Mock<IGame>();
        var processName = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bash" : "cmd";

        var p = Process.Start(processName);
        p.Kill();
        var gp = new GameProcess(p, new GameProcessInfo(game.Object, GameBuildType.Debug, ArgumentCollection.Empty));
        var tcs = new TaskCompletionSource<bool>();
        gp.Closed += (_, _) =>
        {
            tcs.SetResult(true);
        };
        Assert.True(await tcs.Task);
    }
}