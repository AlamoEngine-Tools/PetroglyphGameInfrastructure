using System;

namespace AET.SteamAbstraction.Vdf;

internal class VdfException : SteamException
{
    public VdfException(string message) : base(message)
    {
    }

    public VdfException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}