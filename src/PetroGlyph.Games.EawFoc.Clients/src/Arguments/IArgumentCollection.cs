using System.Collections.Generic;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Represents a collection of <see cref="IGameArgument"/>s.
/// </summary>
public interface IArgumentCollection : IReadOnlyCollection<IGameArgument>
{
}