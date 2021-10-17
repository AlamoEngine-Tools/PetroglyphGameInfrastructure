using System;
using System.Threading.Tasks;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Steam;

namespace PetroGlyph.Games.EawFoc.Clients.Windows.Test.Steam
{
    public class SteamWrapperIntegrationTest
    {
        //[Fact]
        public void TestGameInstalled()
        {
            var sp = new Mock<IServiceProvider>();
            var steam = new SteamWrapper(sp.Object);

            var notInstalled = steam.IsGameInstalled(0, out _);
            var installed = steam.IsGameInstalled(32472, out _);
        }

        //[Fact]
        public void Running()
        {
            var sp = new Mock<IServiceProvider>();
            var steam = new SteamWrapper(sp.Object);

            var running = steam.IsRunning;
        }

        //[Fact]
        public async Task WaitRunning()
        {
            var sp = new Mock<IServiceProvider>();
            var steam = new SteamWrapper(sp.Object);

            await steam.WaitSteamRunningAndLoggedInAsync(false);
        }
    }
}
