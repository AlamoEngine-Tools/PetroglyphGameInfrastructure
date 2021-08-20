using System.IO.Abstractions;
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
        bool IsGameInstalled(uint gameId, [MaybeNullWhen(false)] out IDirectoryInfo location);
#else
        bool IsGameInstalled(uint gameId, out IDirectoryInfo? location);
#endif


        void StartSteam();

        Task WaitSteamRunningAndLoggedInAsync(bool startIfNotRunning, CancellationToken cancellation = default);
    }
}