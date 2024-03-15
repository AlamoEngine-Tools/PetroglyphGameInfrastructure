using System.IO.Abstractions;

namespace AET.SteamAbstraction;

internal interface ISteamAppManifestReader
{
    SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library);
}