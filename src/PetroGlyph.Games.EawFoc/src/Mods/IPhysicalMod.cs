namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Represents an <see cref="IMod"/> which can be located on the file system.
/// </summary>
public interface IPhysicalMod : IMod, IPhysicalPlayableObject;