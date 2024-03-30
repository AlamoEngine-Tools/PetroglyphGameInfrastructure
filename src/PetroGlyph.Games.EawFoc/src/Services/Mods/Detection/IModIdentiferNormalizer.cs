using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

internal interface IModIdentifierBuilder
{
    string Build(IDirectoryInfo modDirectory, bool isWorkshop);

    string Build(IMod mod);

    ModReference Normalize(IModReference modReference);
}