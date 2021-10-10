using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments
{
    public interface IGameArgumentCollection : IEnumerable<IGameArgument>
    {
        IReadOnlyCollection<IGameArgument> Arguments { get; }

        bool AddArgument(IGameArgument argument);

        IGameArgument? RemoveArgument(IGameArgument argument);
    }
}