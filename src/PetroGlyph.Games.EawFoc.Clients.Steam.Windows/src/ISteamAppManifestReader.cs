using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam;

internal interface ISteamAppManifestReader
{
    SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library);
}