using EawModinfo.Model;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    internal interface IModIdentifierBuilder
    {
        string Build(IMod mod);

        ModReference Normalize(IModReference modReference);
    }
}