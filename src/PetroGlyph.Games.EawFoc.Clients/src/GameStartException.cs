using System;

namespace PetroGlyph.Games.EawFoc.Clients;

public class GameStartException : ClientException
{
    public IPlayableObject Instance { get; }

    public GameStartException(IPlayableObject instance, string message, Exception inner) : base(message, inner)
    {
        Instance = instance;
    }

    public GameStartException(IPlayableObject instance, string message) : base(message)
    {
        Instance = instance;
    }

    public GameStartException(IPlayableObject instance)
    {
        Instance = instance;
    }
}