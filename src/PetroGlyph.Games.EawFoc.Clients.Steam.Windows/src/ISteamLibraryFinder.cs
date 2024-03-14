using System.Collections.Generic;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal interface ISteamLibraryFinder
{
    IEnumerable<ISteamLibrary> FindLibraries();
}