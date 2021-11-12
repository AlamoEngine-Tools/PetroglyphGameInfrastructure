namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public enum ArgumentValidityStatus
{
    Valid,
    InvalidName,
    IllegalCharacter,
    PathContainsSpaces,
    EmptyData,
    InvalidData
}