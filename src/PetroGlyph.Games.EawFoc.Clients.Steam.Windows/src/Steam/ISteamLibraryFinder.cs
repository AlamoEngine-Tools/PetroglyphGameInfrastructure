using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal interface ISteamLibraryFinder
{
    IEnumerable<ISteamLibrary> FindLibraries();
}