using System.Threading;
using System.Threading.Tasks;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public interface ISteamWrapper
    {
        bool Installed { get; }

        bool IsRunning { get; }

        bool? WantOfflineMode { get; }

#if NET
        bool IsGameInstalled(uint gameId, [NotNullWhen(true)] out SteamAppManifest manifest);
#else
        bool IsGameInstalled(uint gameId, out SteamAppManifest? manifest);
#endif


        void StartSteam();

        Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning, CancellationToken cancellation = default);
    }
}