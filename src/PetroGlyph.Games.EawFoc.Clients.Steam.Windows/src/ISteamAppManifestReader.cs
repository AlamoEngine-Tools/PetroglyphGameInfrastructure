using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Clients.Steam;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal interface ISteamAppManifestReader
{
    SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library);
}