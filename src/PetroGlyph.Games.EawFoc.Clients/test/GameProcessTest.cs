using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test
{
    public class GameProcessTest
    {
        [Fact]
        public async Task TestOnExit()
        {
            var game = new Mock<IGame>();
            var p = Process.Start("cmd");
            var gp = new GameProcess(p, new GameProcessInfo(game.Object, GameBuildType.Debug, new EmptyArgumentsCollection()));
            var tcs = new TaskCompletionSource<bool>();
            
            gp.Closed += (_, _) => {
                tcs.SetResult(true);
            };
            p.Kill();
            var cts = new CancellationTokenSource(5000);
            using (cts.Token.Register(() => tcs.TrySetCanceled(cts.Token))) 
                await tcs.Task.ConfigureAwait(false);
            Assert.True(tcs.Task.Result);
        }

        [Fact]
        public async Task TestAlreadyExited()
        {
            var game = new Mock<IGame>();
            var p = Process.Start("cmd");
            p.Kill();
            var gp = new GameProcess(p, new GameProcessInfo(game.Object, GameBuildType.Debug, new EmptyArgumentsCollection()));
            var tcs = new TaskCompletionSource<bool>();
            gp.Closed += (_, _) =>
            {
                tcs.SetResult(true);
            };
            await tcs.Task.ConfigureAwait(false);
            Assert.True(tcs.Task.Result);
        }
    }
}
