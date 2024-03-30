using System.Collections.Generic;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Represents a collection of <see cref="IGameArgument"/>s.
/// </summary>
public interface IArgumentCollection : IReadOnlyCollection<IGameArgument>
{
}