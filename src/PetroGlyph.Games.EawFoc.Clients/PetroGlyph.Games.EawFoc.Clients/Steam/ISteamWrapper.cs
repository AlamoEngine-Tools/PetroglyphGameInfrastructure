using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public interface ISteamWrapper
    {
        bool Installed { get; }

        bool IsRunning { get; }

        bool? WantOfflineMode { get; }

        bool IsGameInstalled(uint gameId, out IDirectoryInfo location);

        void StartSteam();

        Task WaitSteamRunningAndLoggedInAsync(CancellationToken cancellation = default);

        Task WaitSteamUserLoggedInAsync(CancellationToken token = default);
    }
}