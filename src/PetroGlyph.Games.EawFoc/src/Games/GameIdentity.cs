namespace PetroGlyph.Games.EawFoc.Games;

/// <summary>
/// Minimal information to identify and distinguish Petroglyph Star Wars games from each other.
/// </summary>
public sealed record GameIdentity(GameType Type, GamePlatform Platform) : IGameIdentity;