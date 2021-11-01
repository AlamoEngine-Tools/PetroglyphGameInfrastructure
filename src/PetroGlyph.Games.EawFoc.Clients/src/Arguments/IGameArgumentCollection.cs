using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public interface IGameArgumentCollection : IReadOnlyCollection<IGameArgument>
{ 
    string ToCommandlineString();
}