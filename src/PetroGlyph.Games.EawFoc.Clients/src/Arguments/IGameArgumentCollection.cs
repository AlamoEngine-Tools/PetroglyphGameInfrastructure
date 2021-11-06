using System.Collections.Generic;
using System.Security;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public interface IGameArgumentCollection : IReadOnlyCollection<IGameArgument>
{
    /// <summary>
    /// Converts this collection into a string which can be used as argument sequence for a Petroglyph Star Wars game.
    /// </summary>
    /// <returns>Strings representation of arguments</returns>
    /// <remarks>This method is required to either sanitize vulnerable inputs or throw a <see cref="SecurityException"/>.</remarks>
    /// <exception cref="SecurityException">
    /// This collection contained arguments which are vulnerable to command injection attacks.
    /// </exception>
    string ToCommandlineString();
}