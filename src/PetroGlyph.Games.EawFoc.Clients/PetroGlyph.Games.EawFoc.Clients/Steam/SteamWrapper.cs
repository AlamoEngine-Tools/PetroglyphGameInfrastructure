using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    internal class SteamWrapper : ISteamWrapper
    {
        public bool Installed { get; }
        public bool IsRunning { get; }
        public bool? WantOfflineMode { get; }

        public bool IsGameInstalled(uint gameId, out IDirectoryInfo location)
        {
            throw new System.NotImplementedException();
        }

        public void StartSteam()
        {
            throw new System.NotImplementedException();
        }

        public Task WaitSteamRunningAndLoggedInAsync(CancellationToken cancellation = default)
        {
            throw new System.NotImplementedException();
        }

        public Task WaitSteamUserLoggedInAsync(CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
