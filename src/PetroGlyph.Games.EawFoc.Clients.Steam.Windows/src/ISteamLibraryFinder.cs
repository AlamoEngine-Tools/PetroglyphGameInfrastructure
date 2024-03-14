using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Clients.Steam;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal interface ISteamLibraryFinder
{
    IEnumerable<ISteamLibrary> FindLibraries();
}