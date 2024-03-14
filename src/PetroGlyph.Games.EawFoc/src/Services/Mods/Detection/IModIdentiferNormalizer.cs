using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

internal interface IModIdentifierBuilder
{
    string Build(IDirectoryInfo modDirectory, bool isWorkshop);

    string Build(IMod mod);

    ModReference Normalize(IModReference modReference);
}