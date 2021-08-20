using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Steam
{
    public class SteamGameLocationFinder : ISteamGameLocationFinder
    {
        private readonly IFileSystem _fileSystem;

        public SteamGameLocationFinder(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _fileSystem = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
        }

        public IDirectoryInfo? FindGame(IDirectoryInfo steamInstallationDirectory, uint gameId)
        {
            Requires.NotNull(steamInstallationDirectory, nameof(steamInstallationDirectory));
            throw new NotImplementedException();
        }
    }
}