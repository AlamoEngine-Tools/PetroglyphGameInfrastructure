using System.IO.Abstractions;
using AET.SteamAbstraction.Library;

namespace AET.SteamAbstraction;

internal interface ISteamAppManifestReader
{
    SteamAppManifest ReadManifest(IFileInfo manifestFile, ISteamLibrary library);
}