using System;

namespace PetroGlyph.Games.EawFoc.Clients;

public class ClientException : Exception
{
    public ClientException()
    {
    }

    public ClientException(string message) : base(message)
    {
    }

    public ClientException(string message, Exception inner) : base(message, inner)
    {
    }
}