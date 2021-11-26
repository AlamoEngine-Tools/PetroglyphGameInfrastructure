using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Clients.Steam;

internal interface ISteamAppManifestReader
{
    SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library);
}