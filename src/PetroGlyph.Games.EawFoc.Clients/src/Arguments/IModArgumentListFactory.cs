using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public interface IModArgumentListFactory
{
    IGameArgument<IReadOnlyList<IGameArgument<string>>> BuildArgumentList(IMod modInstance);

    IGameArgument<IReadOnlyList<IGameArgument<string>>> BuildArgumentList(IList<IMod> traversedModChain);
}